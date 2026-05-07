using GodHandTrainer.Core;
using GodHandTrainer.Data;

namespace GodHandTrainer.Cheats;

public sealed class CombatHooksCheats
{
    private const string HitBox1Id = "HitBox1";
    private const string HitBox2Id = "HitBox2";
    private const string MovePower1Id = "MovePower1";
    private const string MovePower2Id = "MovePower2";
    private const string NoLagId = "NoLagMove";
    private const string OneHitKillId = "OneHitKill";
    private const string UnBlockerId = "UnBlocker";
    private const string NoDamageId = "NoDamage";
    private const string WallsXId = "WalkThroughWallsX";
    private const string WallsZId = "WalkThroughWallsZ";

    private readonly MemoryManager _memory;
    private readonly AOBScanner _scanner;
    private readonly InjectionManager _injection;

    private IntPtr _moveEffectData = IntPtr.Zero;
    private int _currentMoveEffect = Patterns.MoveEffectDefault;

    public CombatHooksCheats(MemoryManager memory, AOBScanner scanner, InjectionManager injection)
    {
        _memory = memory;
        _scanner = scanner;
        _injection = injection;
    }

    public bool HitBoxInstalled => _injection.IsInstalled(HitBox1Id) && _injection.IsInstalled(HitBox2Id);
    public bool DamageTypeInstalled => _injection.IsInstalled(MovePower1Id) && _injection.IsInstalled(MovePower2Id);
    public bool NoLagInstalled => _injection.IsInstalled(NoLagId);
    public bool OneHitKillInstalled => _injection.IsInstalled(OneHitKillId);
    public bool UnBlockerInstalled => _injection.IsInstalled(UnBlockerId);
    public bool NoDamageInstalled => _injection.IsInstalled(NoDamageId);
    public bool WalkThroughWallsInstalled => _injection.IsInstalled(WallsXId) && _injection.IsInstalled(WallsZId);
    public int CurrentMoveEffect => _currentMoveEffect;

    public bool InstallHitBox()
    {
        if (HitBoxInstalled) return true;
        if (!_memory.IsAttached) return false;

        var ok1 = InstallJsHook(HitBox1Id, Patterns.HitBox1Sign, BuildHitBoxTrampoline);
        if (!ok1) return false;
        var ok2 = InstallJsHook(HitBox2Id, Patterns.HitBox2Sign, BuildHitBoxTrampoline);
        if (!ok2)
        {
            _injection.Uninstall(HitBox1Id);
            return false;
        }
        return true;
    }

    public void UninstallHitBox()
    {
        _injection.Uninstall(HitBox1Id);
        _injection.Uninstall(HitBox2Id);
    }

    public bool InstallDamageType()
    {
        if (DamageTypeInstalled) return true;
        if (!_memory.IsAttached) return false;

        if (_moveEffectData == IntPtr.Zero)
        {
            _moveEffectData = _memory.AllocateRemote(4);
            if (_moveEffectData == IntPtr.Zero) return false;
            _memory.WriteBytes(_moveEffectData, BitConverter.GetBytes(_currentMoveEffect));
        }

        var dataAddr = _moveEffectData;
        var ok1 = InstallJsHook(MovePower1Id, Patterns.MovePower1Sign,
            (hookSite, tramp, _, captured) => BuildMovePowerTrampoline(hookSite, tramp, dataAddr, captured));
        if (!ok1)
        {
            FreeMoveEffectIfUnused();
            return false;
        }
        var ok2 = InstallJsHook(MovePower2Id, Patterns.MovePower2Sign,
            (hookSite, tramp, _, captured) => BuildMovePowerTrampoline(hookSite, tramp, dataAddr, captured));
        if (!ok2)
        {
            _injection.Uninstall(MovePower1Id);
            FreeMoveEffectIfUnused();
            return false;
        }
        return true;
    }

    public void UninstallDamageType()
    {
        _injection.Uninstall(MovePower1Id);
        _injection.Uninstall(MovePower2Id);
        FreeMoveEffectIfUnused();
    }

    public bool SetMoveEffect(int value)
    {
        _currentMoveEffect = value;
        if (_moveEffectData == IntPtr.Zero) return false;
        return _memory.WriteBytes(_moveEffectData, BitConverter.GetBytes(value));
    }

    public bool InstallNoLag()
    {
        if (NoLagInstalled) return true;
        if (!_memory.IsAttached) return false;
        return InstallJsHook(NoLagId, Patterns.NoLagMoveSign, BuildNoLagTrampoline);
    }

    public void UninstallNoLag() => _injection.Uninstall(NoLagId);

    public bool InstallOneHitKill()
    {
        if (OneHitKillInstalled) return true;
        if (!_memory.IsAttached) return false;

        var min = (IntPtr)Patterns.TimeScaleScanRangeMin;
        var max = (IntPtr)Patterns.TimeScaleScanRangeMax;
        var hookSite = _scanner.ScanProcess(Patterns.OneHitKillSign, min, max);
        if (hookSite == IntPtr.Zero) return false;

        var captured = new byte[Patterns.OneHitKillPatchSize];
        if (!_memory.ReadBytes(hookSite, captured)) return false;

        return _injection.Install(
            OneHitKillId,
            hookSite,
            Patterns.OneHitKillPatchSize,
            (trampolineAddr, _) => BuildOneHitKillTrampoline(hookSite, trampolineAddr, captured),
            out _,
            out _);
    }

    public void UninstallOneHitKill() => _injection.Uninstall(OneHitKillId);

    public bool InstallUnBlocker()
    {
        if (UnBlockerInstalled) return true;
        if (!_memory.IsAttached) return false;

        var min = (IntPtr)Patterns.TimeScaleScanRangeMin;
        var max = (IntPtr)Patterns.TimeScaleScanRangeMax;
        var aobMatch = _scanner.ScanProcess(Patterns.UnBlockerSign, min, max);
        if (aobMatch == IntPtr.Zero) return false;

        var immediateAddr = IntPtr.Add(aobMatch, Patterns.UnBlockerImmediateOffset);

        return _injection.InstallBytePatch(UnBlockerId, immediateAddr, new byte[] { 0x01 });
    }

    public void UninstallUnBlocker() => _injection.Uninstall(UnBlockerId);

    public bool InstallNoDamage()
    {
        if (NoDamageInstalled) return true;
        if (!_memory.IsAttached) return false;

        var min = (IntPtr)Patterns.TimeScaleScanRangeMin;
        var max = (IntPtr)Patterns.TimeScaleScanRangeMax;
        var hookSite = _scanner.ScanProcess(Patterns.NoDamageSign, min, max);
        if (hookSite == IntPtr.Zero) return false;

        var captured = new byte[Patterns.NoDamagePatchSize];
        if (!_memory.ReadBytes(hookSite, captured)) return false;

        return _injection.Install(
            NoDamageId,
            hookSite,
            Patterns.NoDamagePatchSize,
            (trampolineAddr, _) => BuildNoDamageTrampoline(hookSite, trampolineAddr, captured),
            out _,
            out _);
    }

    public void UninstallNoDamage() => _injection.Uninstall(NoDamageId);

    public bool InstallWalkThroughWalls()
    {
        if (WalkThroughWallsInstalled) return true;
        if (!_memory.IsAttached) return false;

        var ok1 = InstallWallsAxis(WallsXId, Patterns.WalkThroughWallsXSign, Patterns.WalkThroughWallsXPatchSize);
        if (!ok1) return false;
        var ok2 = InstallWallsAxis(WallsZId, Patterns.WalkThroughWallsZSign, Patterns.WalkThroughWallsZPatchSize);
        if (!ok2)
        {
            _injection.Uninstall(WallsXId);
            return false;
        }
        return true;
    }

    public void UninstallWalkThroughWalls()
    {
        _injection.Uninstall(WallsXId);
        _injection.Uninstall(WallsZId);
    }

    private bool InstallWallsAxis(string id, string aob, int patchSize)
    {
        var min = (IntPtr)Patterns.TimeScaleScanRangeMin;
        var max = (IntPtr)Patterns.TimeScaleScanRangeMax;
        var hookSite = _scanner.ScanProcess(aob, min, max);
        if (hookSite == IntPtr.Zero) return false;

        var captured = new byte[patchSize];
        if (!_memory.ReadBytes(hookSite, captured)) return false;

        return _injection.Install(
            id,
            hookSite,
            patchSize,
            (trampolineAddr, _) => BuildWalkThroughWallsTrampoline(hookSite, trampolineAddr, patchSize, captured),
            out _,
            out _);
    }

    private static byte[] BuildWalkThroughWallsTrampoline(IntPtr hookSite, IntPtr trampoline, int patchSize, byte[] captured)
    {
        var buf = new List<byte>(24);

        // mov edx, dword ptr [ecx]   — 8B 11
        // Reads the current position so the captured "mov [ecx], edx" writes the same value back,
        // neutralizing the collision-axis update.
        buf.Add(0x8B);
        buf.Add(0x11);

        buf.AddRange(captured);

        var returnTarget = IntPtr.Add(hookSite, patchSize);
        var jmpFromVa = IntPtr.Add(trampoline, buf.Count);
        var rel = (int)((long)returnTarget - ((long)jmpFromVa + 5));
        buf.Add(0xE9);
        buf.AddRange(BitConverter.GetBytes(rel));
        return buf.ToArray();
    }

    private void FreeMoveEffectIfUnused()
    {
        if (DamageTypeInstalled) return;
        if (_moveEffectData == IntPtr.Zero) return;
        _memory.FreeRemote(_moveEffectData);
        _moveEffectData = IntPtr.Zero;
    }

    private bool InstallJsHook(
        string id,
        string aob,
        Func<IntPtr, IntPtr, IntPtr, byte[], byte[]> buildTrampoline)
    {
        var min = (IntPtr)Patterns.TimeScaleScanRangeMin;
        var max = (IntPtr)Patterns.TimeScaleScanRangeMax;
        var hookSite = _scanner.ScanProcess(aob, min, max);
        if (hookSite == IntPtr.Zero) return false;

        var captured = new byte[Patterns.JsHookPatchSize];
        if (!_memory.ReadBytes(hookSite, captured)) return false;

        return _injection.Install(
            id,
            hookSite,
            Patterns.JsHookPatchSize,
            (trampolineAddr, dataAddr) => buildTrampoline(hookSite, trampolineAddr, dataAddr, captured),
            out _,
            out _);
    }

    private static byte[] BuildHitBoxTrampoline(IntPtr hookSite, IntPtr trampoline, IntPtr _, byte[] captured)
    {
        var buf = new List<byte>(16);

        // mov edx, imm32   — BA [imm32]
        buf.Add(0xBA);
        buf.AddRange(BitConverter.GetBytes(Patterns.HitBoxEdxValue));

        // captured original 6 bytes (the js rel32)
        buf.AddRange(captured);

        AppendJmpBack(buf, trampoline, hookSite);
        return buf.ToArray();
    }

    private static byte[] BuildMovePowerTrampoline(IntPtr hookSite, IntPtr trampoline, IntPtr dataAddr, byte[] captured)
    {
        var buf = new List<byte>(20);

        // mov edx, dword ptr [moveeffect]   — 8B 15 [imm32]
        buf.Add(0x8B);
        buf.Add(0x15);
        buf.AddRange(BitConverter.GetBytes((uint)dataAddr.ToInt64()));

        buf.AddRange(captured);

        AppendJmpBack(buf, trampoline, hookSite);
        return buf.ToArray();
    }

    private static byte[] BuildNoDamageTrampoline(IntPtr hookSite, IntPtr trampoline, byte[] captured)
    {
        var buf = new List<byte>(24);

        // mov dx, word ptr [ecx]   — 66 8B 11
        buf.Add(0x66);
        buf.Add(0x8B);
        buf.Add(0x11);

        buf.AddRange(captured);

        var returnTarget = IntPtr.Add(hookSite, Patterns.NoDamagePatchSize);
        var jmpFromVa = IntPtr.Add(trampoline, buf.Count);
        var rel = (int)((long)returnTarget - ((long)jmpFromVa + 5));
        buf.Add(0xE9);
        buf.AddRange(BitConverter.GetBytes(rel));
        return buf.ToArray();
    }

    private static byte[] BuildOneHitKillTrampoline(IntPtr hookSite, IntPtr trampoline, byte[] captured)
    {
        var buf = new List<byte>(24);

        // mov dx, 0   — 66 BA 00 00
        buf.Add(0x66);
        buf.Add(0xBA);
        buf.Add(0x00);
        buf.Add(0x00);

        buf.AddRange(captured);

        var returnTarget = IntPtr.Add(hookSite, Patterns.OneHitKillPatchSize);
        var jmpFromVa = IntPtr.Add(trampoline, buf.Count);
        var rel = (int)((long)returnTarget - ((long)jmpFromVa + 5));
        buf.Add(0xE9);
        buf.AddRange(BitConverter.GetBytes(rel));
        return buf.ToArray();
    }

    private static byte[] BuildNoLagTrampoline(IntPtr hookSite, IntPtr trampoline, IntPtr _, byte[] captured)
    {
        var buf = new List<byte>(16);

        // mov dx, 0   — 66 BA 00 00
        buf.Add(0x66);
        buf.Add(0xBA);
        buf.Add(0x00);
        buf.Add(0x00);

        buf.AddRange(captured);

        AppendJmpBack(buf, trampoline, hookSite);
        return buf.ToArray();
    }

    private static void AppendJmpBack(List<byte> buf, IntPtr trampoline, IntPtr hookSite)
    {
        var returnTarget = IntPtr.Add(hookSite, Patterns.JsHookPatchSize);
        var jmpFromVa = IntPtr.Add(trampoline, buf.Count);
        var rel = (int)((long)returnTarget - ((long)jmpFromVa + 5));
        buf.Add(0xE9);
        buf.AddRange(BitConverter.GetBytes(rel));
    }
}
