using System.Diagnostics;
using System.Runtime.InteropServices;
using static GodHandTrainer.Core.NativeMethods;

namespace GodHandTrainer.Core;

public sealed class MemoryManager
{
    private static readonly Lazy<MemoryManager> _instance = new(() => new MemoryManager());
    public static MemoryManager Instance => _instance.Value;

    private const uint EE_RAM_SIZE = 0x02000000;
    private const ulong PS2_KSEG_MASK = 0x1FFFFFFF;

    private IntPtr _handle = IntPtr.Zero;
    private Process? _process;
    private IntPtr _eeBase = IntPtr.Zero;

    public bool IsAttached => _handle != IntPtr.Zero && _process is { HasExited: false };
    public Process? Process => _process;
    public IntPtr EEBase => _eeBase;

    public event EventHandler? AttachStateChanged;

    private MemoryManager() { }

    public bool TryAttach(string processName = "pcsx2")
    {
        if (IsAttached) return true;

        var procs = Process.GetProcessesByName(processName);
        if (procs.Length == 0) return false;

        var proc = procs[0];
        var handle = OpenProcess(PROCESS_ALL_ACCESS, false, proc.Id);
        if (handle == IntPtr.Zero) return false;

        _process = proc;
        _handle = handle;

        if (!TryResolveEEBase(out _eeBase))
        {
            Detach();
            return false;
        }

        AttachStateChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public void Detach()
    {
        if (_handle != IntPtr.Zero)
        {
            CloseHandle(_handle);
            _handle = IntPtr.Zero;
        }
        _process = null;
        _eeBase = IntPtr.Zero;
        AttachStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public IntPtr ResolvePS2(uint ps2Address)
    {
        if (_eeBase == IntPtr.Zero) return IntPtr.Zero;
        var offset = ps2Address & (uint)PS2_KSEG_MASK;
        return IntPtr.Add(_eeBase, (int)offset);
    }

    public bool ReadBytes(IntPtr address, byte[] buffer)
    {
        EnsureAttached();
        return ReadProcessMemory(_handle, address, buffer, buffer.Length, out var read) && read == buffer.Length;
    }

    public bool WriteBytes(IntPtr address, byte[] buffer)
    {
        EnsureAttached();
        return WriteProcessMemory(_handle, address, buffer, buffer.Length, out var written) && written == buffer.Length;
    }

    public int ReadInt32(uint ps2Address) => BitConverter.ToInt32(ReadAt(ps2Address, 4), 0);
    public uint ReadUInt32(uint ps2Address) => BitConverter.ToUInt32(ReadAt(ps2Address, 4), 0);
    public short ReadInt16(uint ps2Address) => BitConverter.ToInt16(ReadAt(ps2Address, 2), 0);
    public byte ReadByte(uint ps2Address) => ReadAt(ps2Address, 1)[0];
    public float ReadFloat(uint ps2Address) => BitConverter.ToSingle(ReadAt(ps2Address, 4), 0);

    public bool WriteInt32(uint ps2Address, int value) => WriteBytes(ResolvePS2(ps2Address), BitConverter.GetBytes(value));
    public bool WriteInt64(uint ps2Address, long value) => WriteBytes(ResolvePS2(ps2Address), BitConverter.GetBytes(value));
    public bool WriteUInt32(uint ps2Address, uint value) => WriteBytes(ResolvePS2(ps2Address), BitConverter.GetBytes(value));
    public bool WriteInt16(uint ps2Address, short value) => WriteBytes(ResolvePS2(ps2Address), BitConverter.GetBytes(value));
    public bool WriteByte(uint ps2Address, byte value) => WriteBytes(ResolvePS2(ps2Address), new[] { value });
    public bool WriteFloat(uint ps2Address, float value) => WriteBytes(ResolvePS2(ps2Address), BitConverter.GetBytes(value));
    public bool WriteByteArray(uint ps2Address, byte[] data) => WriteBytes(ResolvePS2(ps2Address), data);

    private byte[] ReadAt(uint ps2Address, int size)
    {
        var buffer = new byte[size];
        ReadBytes(ResolvePS2(ps2Address), buffer);
        return buffer;
    }

    public IntPtr AllocateRemote(uint size, uint protect = PAGE_READWRITE)
    {
        EnsureAttached();
        return VirtualAllocEx(_handle, IntPtr.Zero, size, MEM_COMMIT | MEM_RESERVE, protect);
    }

    public bool FreeRemote(IntPtr address)
    {
        if (!IsAttached || address == IntPtr.Zero) return false;
        return VirtualFreeEx(_handle, address, 0, MEM_RELEASE);
    }

    private void EnsureAttached()
    {
        if (!IsAttached)
            throw new InvalidOperationException("Trainer is not attached to pcsx2.exe.");
    }

    internal IEnumerable<MEMORY_BASIC_INFORMATION> EnumerateRegions(IntPtr min, IntPtr max)
    {
        EnsureAttached();
        var addr = min;
        var mbiSize = (uint)Marshal.SizeOf<MEMORY_BASIC_INFORMATION>();

        while ((long)addr < (long)max)
        {
            if (VirtualQueryEx(_handle, addr, out var mbi, mbiSize) == 0) yield break;
            yield return mbi;
            var next = (long)mbi.BaseAddress + (long)mbi.RegionSize;
            if (next <= (long)addr) yield break;
            addr = (IntPtr)next;
        }
    }

    private bool TryResolveEEBase(out IntPtr baseAddress)
    {
        baseAddress = (IntPtr)0x20000000;
        return true;
    }
}
