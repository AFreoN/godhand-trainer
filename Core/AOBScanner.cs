using static GodHandTrainer.Core.NativeMethods;

namespace GodHandTrainer.Core;

public sealed class AOBScanner
{
    private readonly MemoryManager _memory;

    public AOBScanner(MemoryManager memory) => _memory = memory;

    public IntPtr ScanProcess(string pattern, IntPtr min, IntPtr max)
    {
        if (!_memory.IsAttached) return IntPtr.Zero;
        var (bytes, mask) = ParsePattern(pattern);

        foreach (var region in _memory.EnumerateRegions(min, max))
        {
            if (region.State != MEM_COMMIT) continue;
            if ((region.Protect & PAGE_GUARD) != 0) continue;
            if ((region.Protect & (PAGE_NOACCESS | PAGE_WRITECOPY)) != 0) continue;

            var size = (long)region.RegionSize;
            if (size <= 0) continue;

            var buffer = new byte[size];
            if (!_memory.ReadBytes(region.BaseAddress, buffer)) continue;

            var idx = IndexOfPattern(buffer, bytes, mask);
            if (idx >= 0) return IntPtr.Add(region.BaseAddress, idx);
        }
        return IntPtr.Zero;
    }

    public static (byte[] bytes, byte[] mask) ParsePattern(string pattern)
    {
        var tokens = pattern.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var bytes = new byte[tokens.Length];
        var mask = new byte[tokens.Length];
        for (var i = 0; i < tokens.Length; i++)
        {
            var t = tokens[i];
            if (t.Length == 1) t = t == "?" ? "??" : "0" + t;
            if (t.Length != 2) throw new FormatException($"Invalid AOB token '{tokens[i]}'.");

            byte b = 0, m = 0;
            for (var k = 0; k < 2; k++)
            {
                var c = t[k];
                m <<= 4;
                b <<= 4;
                if (c == '?') continue;
                m |= 0x0F;
                b |= (byte)HexNibble(c);
            }
            bytes[i] = b;
            mask[i] = m;
        }
        return (bytes, mask);
    }

    private static int HexNibble(char c) => c switch
    {
        >= '0' and <= '9' => c - '0',
        >= 'a' and <= 'f' => c - 'a' + 10,
        >= 'A' and <= 'F' => c - 'A' + 10,
        _ => throw new FormatException($"Invalid hex char '{c}'."),
    };

    private static int IndexOfPattern(byte[] haystack, byte[] needle, byte[] mask)
    {
        var limit = haystack.Length - needle.Length;
        for (var i = 0; i <= limit; i++)
        {
            var match = true;
            for (var j = 0; j < needle.Length; j++)
            {
                var m = mask[j];
                if (m == 0) continue;
                if ((haystack[i + j] & m) != (needle[j] & m))
                {
                    match = false;
                    break;
                }
            }
            if (match) return i;
        }
        return -1;
    }
}
