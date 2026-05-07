namespace GodHandTrainer.Data;

public readonly record struct MoveDefinition(int Id, string Name)
{
    public override string ToString() => $"{Id:D3} — {Name}";
}

public readonly record struct CombatSlot(string Label, uint Address);

public static class MoveCatalog
{
    public static readonly IReadOnlyList<MoveDefinition> Moves = new MoveDefinition[]
    {
        new(0, "Left Jab 1"), new(1, "Left Jab 2"), new(2, "Left Jab 3"),
        new(3, "Mach Speed Jab 1"), new(4, "Straight 1"), new(5, "Straight 2"),
        new(6, "Straight 3"), new(7, "Long Straight 1"), new(8, "Long Straight 2"),
        new(9, "Long Straight 3"), new(10, "Left Hook 1"), new(11, "Left Hook 2"),
        new(12, "Left Hook 3"), new(13, "Uppercut 1"), new(14, "Uppercut 2"),
        new(15, "Uppercut 3"), new(16, "Chop"), new(17, "Elbow Spin 1"),
        new(18, "Elbow Spin 2"), new(19, "Elbow Spin 3"), new(20, "Pay Up"),
        new(21, "Really Pay Up"), new(22, "Pay Up NOW!"), new(23, "Pimp Hand"),
        new(24, "Pimp Smack"), new(25, "Godly Smack"), new(26, "Low Kick 1"),
        new(27, "Low Kick 2"), new(28, "Godly Low Kick"), new(29, "Right Roundhouse 1"),
        new(30, "Right Roundhouse 2"), new(31, "Right Roundhouse 3"), new(32, "Left Roundhouse"),
        new(33, "Spinning Backfist"), new(34, "Backfist Strike 1"), new(35, "Double Spin Kick"),
        new(36, "Mule Kick"), new(37, "High Side Kick 1"), new(38, "Rolling Sobat"),
        new(39, "Haymaker 1"), new(40, "High Kick"), new(41, "Knee Strike"),
        new(42, "High Snap Kick"), new(43, "Triple Side Kick"), new(44, "Flying Triple"),
        new(45, "Step Back Kick"), new(46, "Fist of Justice"), new(47, "Reverse Sweep"),
        new(48, "Back Roundhouse"), new(49, "Spinning Sobat"), new(50, "Somersault"),
        new(51, "One-Two Punch"), new(52, "Barrell Roll Kick"), new(53, "Expert Sobat"),
        new(54, "Dashing Sobat"), new(55, "Hand Plant Kick"), new(56, "High Cross Kick"),
        new(57, "Flying Knee"), new(58, "Elbow Vortex"), new(59, "Forearm Smash 1"),
        new(60, "Palm Smash"), new(61, "Reverse Hell Kick"), new(62, "Double Snap Kick"),
        new(63, "Sugar Gene Combo"), new(64, "Floating Butterfly"), new(65, "Stinging Bee"),
        new(66, "Punch Rush"), new(67, "Chin Music"), new(68, "Invincible Fist"),
        new(69, "Right High Knee"), new(70, "Drunken Twist"), new(71, "Drunken Fist 1"),
        new(72, "Drunken Sweep"), new(73, "Toe Touch Kick"), new(74, "Drunken Fall"),
        new(75, "Heel Drop"), new(76, "Side Swipe"), new(77, "Chin Rocker"),
        new(78, "Rocket Uppercut"), new(79, "Overhead Blow"), new(80, "Stomping Fist"),
        new(81, "Right Twister"), new(82, "Left Twister"), new(83, "Half Moon Kick"),
        new(84, "Guard Breaker 1"), new(85, "Charged Punch 1"), new(86, "Short Uppercut 1"),
        new(87, "Short Uppercut 2"), new(88, "Short Uppercut 3"), new(89, "Right Hook 1"),
        new(90, "Right Hook 2"), new(91, "Right Hook 3"), new(92, "Granny Smacker"),
        new(93, "Yes Man Kabalaam"), new(94, "Jab of God"), new(95, "Godly Straight"),
        new(96, "God Hook"), new(97, "God Uppercut"), new(98, "Godly Chop"),
        new(99, "Fist of God"), new(100, "High Side Kick 2"), new(101, "High Side Kick 3"),
        new(102, "Mach Speed Jab 2"), new(103, "Forearm Smash 2"), new(104, "Haymaker 2"),
        new(105, "Haymaker of God"), new(106, "Guard Breaker 2"), new(107, "God Breaker"),
        new(108, "Backfist Strike 2"), new(109, "Drunken Fist 2"), new(110, "Charged Punch 2"),
        new(111, "Charged Punch 3"), new(112, "Charged Punch 4"), new(113, "God Charged Punch"),
    };

    public static readonly IReadOnlyList<CombatSlot> Slots = new CombatSlot[]
    {
        new("Triangle", Offsets.MoveTriangle),
        new("Down Triangle", Offsets.MoveDownTriangle),
        new("Cross", Offsets.MoveCross),
        new("Down Cross", Offsets.MoveDownCross),
        new("Down Square", Offsets.MoveDownSquare),
        new("Square Attack 1", Offsets.MoveSquare1),
        new("Square Attack 2", Offsets.MoveSquare2),
        new("Square Attack 3", Offsets.MoveSquare3),
        new("Square Attack 4", Offsets.MoveSquare4),
        new("Square Attack 5", Offsets.MoveSquare5),
        new("Square Attack 6", Offsets.MoveSquare6),
    };
}
