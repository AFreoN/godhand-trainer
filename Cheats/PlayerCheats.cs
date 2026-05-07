using GodHandTrainer.Core;
using GodHandTrainer.Data;

namespace GodHandTrainer.Cheats;

public sealed class PlayerCheats
{
    private readonly MemoryManager _memory;

    public PlayerCheats(MemoryManager memory) => _memory = memory;

    public bool SetGold(int amount)
    {
        var clamped = Math.Clamp(amount, Offsets.GoldMin, Offsets.GoldMax);
        return _memory.WriteInt32(Offsets.Gold, clamped);
    }

    public int? GetGold()
    {
        if (!_memory.IsAttached) return null;
        try { return _memory.ReadInt32(Offsets.Gold); }
        catch { return null; }
    }

    public bool SetGodMode(bool enabled)
        => _memory.WriteInt32(Offsets.GodMode, enabled ? 1 : 0);

    public bool? GetGodMode()
    {
        if (!_memory.IsAttached) return null;
        try { return _memory.ReadInt32(Offsets.GodMode) != 0; }
        catch { return null; }
    }

    public bool SetGodHandMeter(int value)
    {
        var clamped = (short)Math.Clamp(value, Offsets.GodHandMeterMin, Offsets.GodHandMeterMax);
        return _memory.WriteInt16(Offsets.GodHandMeter, clamped);
    }

    public int? GetGodHandMeter()
    {
        if (!_memory.IsAttached) return null;
        try { return _memory.ReadInt16(Offsets.GodHandMeter); }
        catch { return null; }
    }

    public bool SetLevelMeter(int value)
    {
        var clamped = (short)Math.Clamp(value, Offsets.LevelMeterMin, Offsets.LevelMeterMax);
        return _memory.WriteInt16(Offsets.LevelMeter, clamped);
    }

    public int? GetLevelMeter()
    {
        if (!_memory.IsAttached) return null;
        try { return _memory.ReadInt16(Offsets.LevelMeter); }
        catch { return null; }
    }

    public bool SetUnlimitedKeys()
    {
        var success = true;
        success &= _memory.WriteInt32(Offsets.UnlimitedKeys1, Offsets.UnlimitedKeysValue);
        success &= _memory.WriteInt32(Offsets.UnlimitedKeys2, Offsets.UnlimitedKeysValue);
        success &= _memory.WriteInt32(Offsets.UnlimitedKeys3, Offsets.UnlimitedKeys3Value);
        return success;
    }
}
