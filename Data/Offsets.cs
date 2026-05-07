namespace GodHandTrainer.Data;

public static class Offsets
{
    public const uint Gold = 0x205686F0;

    public const uint GodMode = 0x205CB010;
    public const uint GodHandMeter = 0x205CB006;
    public const uint LevelMeter = 0x205686EC;

    public const uint UnlimitedKeys1 = 0x20568738;
    public const uint UnlimitedKeys2 = 0x2056873C;
    public const uint UnlimitedKeys3 = 0x208C2AF4;
    public const int UnlimitedKeysValue = 3;

    public const int GodHandMeterMin = 0;
    public const int GodHandMeterMax = 17172;
    public const int LevelMeterMin = 0;
    public const int LevelMeterMax = 5000;

    public const uint AllRoulettesUnlock = 0x20568800;
    public const uint RouletteSlots = 0x20568826;
    public const uint RouletteSlotsHolder = 0x20568768;
    public const uint DoubleGodHand = 0x20568778;

    public const byte RouletteSlotsMin = 2;
    public const byte RouletteSlotsMax = 6;

    public const long DoubleGodHandDefault = 1103823438081L;
    public const long DoubleGodHandKarate = 361696448896631041L;
    public const long DoubleGodHandDevil = 217018310867353857L;

    public const uint MoveTriangle = 0x205688A4;
    public const uint MoveDownTriangle = 0x20568910;
    public const uint MoveCross = 0x205688C8;
    public const uint MoveDownCross = 0x20568934;
    public const uint MoveDownSquare = 0x205688EC;
    public const uint MoveSquare1 = 0x20568880;
    public const uint MoveSquare2 = 0x20568884;
    public const uint MoveSquare3 = 0x20568888;
    public const uint MoveSquare4 = 0x2056888C;
    public const uint MoveSquare5 = 0x20568890;
    public const uint MoveSquare6 = 0x20568894;

    public const uint MovesUnlocker = 0x20568780;
    public const int MovesUnlockerLength = 114;

    public const int GoldMin = 0;
    public const int GoldMax = 999_999;

    public const int AllRoulettesUnlockValue = 1_862_270_975;

    public static readonly byte[] MovesUnlockerPayload =
    {
        0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0,
        1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1,
        1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
    };
}
