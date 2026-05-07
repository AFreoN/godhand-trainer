using GodHandTrainer.Core;

namespace GodHandTrainer.Cheats;

public sealed class CombatCheats
{
    private readonly MemoryManager _memory;

    public CombatCheats(MemoryManager memory) => _memory = memory;

    public bool SetSlotMove(uint address, int moveId)
        => _memory.WriteInt32(address, moveId);

    public int? GetSlotMove(uint address)
    {
        if (!_memory.IsAttached) return null;
        try { return _memory.ReadInt32(address); }
        catch { return null; }
    }
}
