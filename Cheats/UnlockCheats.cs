using GodHandTrainer.Core;
using GodHandTrainer.Data;

namespace GodHandTrainer.Cheats;

public sealed class UnlockCheats
{
    private readonly MemoryManager _memory;

    public UnlockCheats(MemoryManager memory) => _memory = memory;

    public bool UnlockAllMoves()
        => _memory.WriteByteArray(Offsets.MovesUnlocker, Offsets.MovesUnlockerPayload);

    public bool UnlockAllRoulettes()
        => _memory.WriteInt32(Offsets.AllRoulettesUnlock, Offsets.AllRoulettesUnlockValue);

    public bool SetDoubleGodHand(DoubleGodHandMode mode)
        => _memory.WriteInt64(Offsets.DoubleGodHand, mode switch
        {
            DoubleGodHandMode.Karate => Offsets.DoubleGodHandKarate,
            DoubleGodHandMode.Devil => Offsets.DoubleGodHandDevil,
            _ => Offsets.DoubleGodHandDefault,
        });

    public bool SetRouletteSlots(byte count)
    {
        var clamped = Math.Clamp(count, Offsets.RouletteSlotsMin, Offsets.RouletteSlotsMax);
        return _memory.WriteByte(Offsets.RouletteSlots, clamped);
    }
}

public enum DoubleGodHandMode
{
    Default,
    Karate,
    Devil,
}
