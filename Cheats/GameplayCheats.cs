using GodHandTrainer.Core;
using GodHandTrainer.Data;

namespace GodHandTrainer.Cheats;

public sealed class GameplayCheats
{
    private const string TimeScaleHookId = "TimeScaler";

    public const float MinTimeScale = 0.5f;
    public const float MaxTimeScale = 10f;

    private readonly MemoryManager _memory;
    private readonly AOBScanner _scanner;
    private readonly InjectionManager _injection;

    private IntPtr _playerSpeedAddress = IntPtr.Zero;
    private float _currentTimeScale = 1f;

    public GameplayCheats(MemoryManager memory, AOBScanner scanner, InjectionManager injection)
    {
        _memory = memory;
        _scanner = scanner;
        _injection = injection;
    }

    public bool TimeScaleInstalled => _injection.IsInstalled(TimeScaleHookId);
    public float CurrentTimeScale => _currentTimeScale;

    public bool InstallTimeScale()
    {
        if (TimeScaleInstalled) return true;
        if (!_memory.IsAttached) return false;

        var min = (IntPtr)Patterns.TimeScaleScanRangeMin;
        var max = (IntPtr)Patterns.TimeScaleScanRangeMax;
        var found = _scanner.ScanProcess(Patterns.TimeScaleSign, min, max);
        if (found == IntPtr.Zero) return false;

        var hookSite = IntPtr.Add(found, Patterns.TimeScaleHookSiteOffset);
        var originalBytes = new byte[Patterns.TimeScaleHookPatchSize];
        if (!_memory.ReadBytes(hookSite, originalBytes)) return false;

        var ok = _injection.Install(
            TimeScaleHookId,
            hookSite,
            Patterns.TimeScaleHookPatchSize,
            (trampolineAddr, dataAddr) => BuildTimeScaleTrampoline(hookSite, trampolineAddr, dataAddr, originalBytes),
            out _,
            out var dataAddress,
            dataSize: 4);

        if (!ok) return false;

        _playerSpeedAddress = dataAddress;
        SetTimeScale(_currentTimeScale);
        return true;
    }

    public void UninstallTimeScale()
    {
        if (!TimeScaleInstalled) return;
        _injection.Uninstall(TimeScaleHookId);
        _playerSpeedAddress = IntPtr.Zero;
    }

    public bool SetTimeScale(float value)
    {
        var clamped = Math.Clamp(value, MinTimeScale, MaxTimeScale);
        _currentTimeScale = clamped;
        if (!TimeScaleInstalled || _playerSpeedAddress == IntPtr.Zero) return false;
        return _memory.WriteBytes(_playerSpeedAddress, BitConverter.GetBytes(clamped));
    }

    private static byte[] BuildTimeScaleTrampoline(
        IntPtr hookSite,
        IntPtr trampolineAddress,
        IntPtr dataAddress,
        byte[] capturedOriginal)
    {
        const int trampolineSize = 35;
        var buf = new byte[trampolineSize];
        var idx = 0;

        // cmp dword ptr [ecx+0x68], 0x0041B800   — bytes: 81 79 68 00 B8 41 00
        buf[idx++] = 0x81;
        buf[idx++] = 0x79;
        buf[idx++] = 0x68;
        BitConverter.GetBytes(Patterns.PlayerActorIdConst).CopyTo(buf, idx);
        idx += 4;

        // jne enemy (rel8 = +8)
        buf[idx++] = 0x75;
        buf[idx++] = 0x08;

        // player: mov edx, dword ptr [PlayerSpeed]   — 8B 15 [imm32]
        buf[idx++] = 0x8B;
        buf[idx++] = 0x15;
        BitConverter.GetBytes((uint)dataAddress.ToInt64()).CopyTo(buf, idx);
        idx += 4;

        // jmp originalcode (rel8 = +5)
        buf[idx++] = 0xEB;
        buf[idx++] = 0x05;

        // enemy: mov edx, 0x3F800000 (float 1.0)    — BA 00 00 80 3F
        buf[idx++] = 0xBA;
        buf[idx++] = 0x00;
        buf[idx++] = 0x00;
        buf[idx++] = 0x80;
        buf[idx++] = 0x3F;

        // originalcode: 8 bytes captured from hook site
        Array.Copy(capturedOriginal, 0, buf, idx, capturedOriginal.Length);
        idx += capturedOriginal.Length;

        // jmp back to hookSite + patchSize
        var returnTarget = IntPtr.Add(hookSite, Patterns.TimeScaleHookPatchSize);
        var jmpFrom = IntPtr.Add(trampolineAddress, idx);
        var rel = (int)((long)returnTarget - ((long)jmpFrom + 5));
        buf[idx++] = 0xE9;
        BitConverter.GetBytes(rel).CopyTo(buf, idx);
        idx += 4;

        if (idx != trampolineSize)
            throw new InvalidOperationException($"Trampoline size mismatch: expected {trampolineSize}, got {idx}.");

        return buf;
    }
}
