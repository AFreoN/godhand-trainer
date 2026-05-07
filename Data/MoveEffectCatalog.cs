namespace GodHandTrainer.Data;

public readonly record struct MoveEffectDefinition(int Id, string Name)
{
    public override string ToString() => $"{Id:D3} — {Name}";
}

public static class MoveEffectCatalog
{
    public static readonly IReadOnlyList<MoveEffectDefinition> Effects = new MoveEffectDefinition[]
    {
        new(15, "Launch"),
        new(10, "Juggle"),
        new(11, "Juggle High"),
        new(28, "Juggle Very High"),
        new(48, "Juggle Very Very High"),
        new(45, "Guard Breaker"),
        new(13, "Unblockable Launch"),
        new(29, "Unblockable Charged Launch & Demon Launch"),
        new(18, "Dodged Juggle"),
        new(4,  "Heat Up"),
        new(3,  "Diggz Enemy"),
        new(22, "Sweep Attack"),
        new(5,  "Enemy Never Try to Block"),
        new(19, "Groundy Deliver"),
        new(50, "Weapon Square"),
        new(51, "Weapon Triangle"),
        new(52, "Stun Weapon Square Attack"),
        new(53, "Stun Weapon Triangle Attack"),
        new(7,  "Drunken / L / R Twist"),
        new(46, "Box Hit Effect"),
        new(30, "Dragon Kick Effect"),
        new(31, "Magical One Hit KO"),
        new(32, "Powerful Launch"),
        new(33, "Enemy Stays on Ground After Hit"),
        new(34, "Ball Buster Hit"),
        new(37, "Head Slicer"),
        new(38, "One Inch Punch"),
        new(40, "Chain Yanker"),
        new(35, "Mid Air"),
        new(39, "Typhoon Kick"),
        new(42, "Spirit"),
    };
}
