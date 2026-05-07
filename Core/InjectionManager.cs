using static GodHandTrainer.Core.NativeMethods;

namespace GodHandTrainer.Core;

public sealed class InjectionManager
{
    private readonly MemoryManager _memory;
    private readonly Dictionary<string, InstalledHook> _hooks = new();

    public InjectionManager(MemoryManager memory) => _memory = memory;

    public bool IsInstalled(string id) => _hooks.ContainsKey(id);

    public bool InstallBytePatch(string id, IntPtr address, byte[] newBytes)
    {
        if (!_memory.IsAttached) return false;
        if (_hooks.ContainsKey(id)) return true;

        var original = new byte[newBytes.Length];
        if (!_memory.ReadBytes(address, original)) return false;
        if (!WriteWithProtect(address, newBytes)) return false;

        _hooks[id] = new InstalledHook
        {
            HookSite = address,
            OriginalBytes = original,
            Trampoline = IntPtr.Zero,
            Data = IntPtr.Zero,
        };
        return true;
    }

    public bool Install(string id, IntPtr hookSite, int patchSize, Func<IntPtr, IntPtr, byte[]> buildTrampoline,
        out IntPtr trampolineAddress, out IntPtr dataAddress, int dataSize = 0)
    {
        trampolineAddress = IntPtr.Zero;
        dataAddress = IntPtr.Zero;
        if (!_memory.IsAttached) return false;
        if (_hooks.ContainsKey(id)) return true;
        if (patchSize < 5) throw new ArgumentException("patchSize must be >= 5 for a relative jmp.", nameof(patchSize));

        var process = _memory.Process!;
        var processHandle = OpenProcessHandle(process.Id);
        if (processHandle == IntPtr.Zero) return false;

        try
        {
            var trampoline = VirtualAllocEx(processHandle, IntPtr.Zero, 2048, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (trampoline == IntPtr.Zero) return false;

            IntPtr data = IntPtr.Zero;
            if (dataSize > 0)
            {
                data = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)dataSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (data == IntPtr.Zero)
                {
                    VirtualFreeEx(processHandle, trampoline, 0, MEM_RELEASE);
                    return false;
                }
            }

            var trampolineCode = buildTrampoline(trampoline, data);
            if (!_memory.WriteBytes(trampoline, trampolineCode))
            {
                VirtualFreeEx(processHandle, trampoline, 0, MEM_RELEASE);
                if (data != IntPtr.Zero) VirtualFreeEx(processHandle, data, 0, MEM_RELEASE);
                return false;
            }

            var original = new byte[patchSize];
            if (!_memory.ReadBytes(hookSite, original))
            {
                VirtualFreeEx(processHandle, trampoline, 0, MEM_RELEASE);
                if (data != IntPtr.Zero) VirtualFreeEx(processHandle, data, 0, MEM_RELEASE);
                return false;
            }

            var patch = BuildJumpPatch(hookSite, trampoline, patchSize);
            if (!WriteWithProtect(hookSite, patch))
            {
                VirtualFreeEx(processHandle, trampoline, 0, MEM_RELEASE);
                if (data != IntPtr.Zero) VirtualFreeEx(processHandle, data, 0, MEM_RELEASE);
                return false;
            }

            _hooks[id] = new InstalledHook
            {
                HookSite = hookSite,
                OriginalBytes = original,
                Trampoline = trampoline,
                Data = data,
                DataSize = dataSize,
            };
            trampolineAddress = trampoline;
            dataAddress = data;
            return true;
        }
        finally
        {
            CloseHandle(processHandle);
        }
    }

    public bool Uninstall(string id)
    {
        if (!_hooks.TryGetValue(id, out var hook)) return false;
        if (!_memory.IsAttached)
        {
            _hooks.Remove(id);
            return true;
        }

        WriteWithProtect(hook.HookSite, hook.OriginalBytes);

        var processHandle = OpenProcessHandle(_memory.Process!.Id);
        if (processHandle != IntPtr.Zero)
        {
            try
            {
                if (hook.Trampoline != IntPtr.Zero)
                    VirtualFreeEx(processHandle, hook.Trampoline, 0, MEM_RELEASE);
                if (hook.Data != IntPtr.Zero)
                    VirtualFreeEx(processHandle, hook.Data, 0, MEM_RELEASE);
            }
            finally
            {
                CloseHandle(processHandle);
            }
        }

        _hooks.Remove(id);
        return true;
    }

    public IntPtr GetDataAddress(string id) => _hooks.TryGetValue(id, out var h) ? h.Data : IntPtr.Zero;

    public void UninstallAll()
    {
        foreach (var id in _hooks.Keys.ToList())
            Uninstall(id);
    }

    private bool WriteWithProtect(IntPtr address, byte[] data)
    {
        var processHandle = OpenProcessHandle(_memory.Process!.Id);
        if (processHandle == IntPtr.Zero) return false;
        try
        {
            if (!VirtualProtectEx(processHandle, address, (uint)data.Length, PAGE_EXECUTE_READWRITE, out var oldProtect))
                return false;
            var ok = _memory.WriteBytes(address, data);
            VirtualProtectEx(processHandle, address, (uint)data.Length, oldProtect, out _);
            return ok;
        }
        finally
        {
            CloseHandle(processHandle);
        }
    }

    private static IntPtr OpenProcessHandle(int pid)
        => OpenProcess(PROCESS_ALL_ACCESS, false, pid);

    public static byte[] BuildJumpPatch(IntPtr from, IntPtr to, int patchSize)
    {
        var patch = new byte[patchSize];
        for (var i = 0; i < patchSize; i++) patch[i] = 0x90;
        var rel = (int)((long)to - ((long)from + 5));
        patch[0] = 0xE9;
        BitConverter.GetBytes(rel).CopyTo(patch, 1);
        return patch;
    }

    public static int RelOffset(IntPtr from, IntPtr to, int instrLen)
        => (int)((long)to - ((long)from + instrLen));

    private sealed class InstalledHook
    {
        public IntPtr HookSite;
        public byte[] OriginalBytes = Array.Empty<byte>();
        public IntPtr Trampoline;
        public IntPtr Data;
        public int DataSize;
    }
}
