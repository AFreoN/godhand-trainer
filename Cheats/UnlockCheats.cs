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
        => _memory.WriteByteArray(Offsets.AllRoulettesUnlock, Offsets.AllRoulettesUnlockPayload);

    public bool SetCostume(Costume costume)
        => _memory.WriteByte(Offsets.Costume, (byte)costume);

    public bool SetRouletteAvailable(byte count)
    {
        var clamped = Math.Clamp(count, Offsets.RouletteAvailableMin, Offsets.RouletteAvailableMax);
        var payload = new byte[Offsets.RouletteAvailableLength];
        for (var i = 0; i < clamped; i++) payload[i] = 1;
        return _memory.WriteByteArray(Offsets.RouletteAvailable, payload);
    }

    public byte? GetRouletteAvailable()
    {
        var buffer = new byte[Offsets.RouletteAvailableLength];
        if (!_memory.ReadBytes(_memory.ResolvePS2(Offsets.RouletteAvailable), buffer)) return null;

        var ones = 0;
        while (ones < buffer.Length && buffer[ones] == 1) ones++;
        for (var i = ones; i < buffer.Length; i++)
            if (buffer[i] != 0) return null;

        return ones is >= Offsets.RouletteAvailableMin and <= Offsets.RouletteAvailableMax ? (byte)ones : null;
    }

    public bool SetRouletteSlots(byte count)
    {
        var clamped = Math.Clamp(count, Offsets.RouletteSlotsMin, Offsets.RouletteSlotsMax);
        return _memory.WriteByte(Offsets.RouletteSlots, clamped);
    }
}

public enum Costume : byte
{
    Original = 0,
    OriginalDouble = 1,
    Devil = 2,
    DevilDouble = 3,
    Karate = 4,
    KarateDouble = 5,
    Carnival = 6,
    CarnivalDouble = 7,
}
