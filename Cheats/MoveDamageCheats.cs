using GodHandTrainer.Core;

namespace GodHandTrainer.Cheats;

public sealed class MoveDamageCheats
{
    private readonly MemoryManager _memory;

    public MoveDamageCheats(MemoryManager memory) => _memory = memory;

    public bool SetDamage(uint address, int value) => _memory.WriteInt32(address, value);

    public int? GetDamage(uint address)
    {
        if (!_memory.IsAttached) return null;
        try { return _memory.ReadInt32(address); }
        catch { return null; }
    }
}
