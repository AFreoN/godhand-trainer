namespace GodHandTrainer.UI;

internal static class Theme
{
    public static readonly Color Background = Color.FromArgb(18, 20, 26);
    public static readonly Color Surface = Color.FromArgb(26, 28, 34);
    public static readonly Color SurfaceAlt = Color.FromArgb(32, 34, 40);
    public static readonly Color Input = Color.FromArgb(54, 57, 68);
    public static readonly Color InputHover = Color.FromArgb(64, 68, 80);
    public static readonly Color InputButton = Color.FromArgb(44, 47, 56);
    public static readonly Color Border = Color.FromArgb(72, 76, 88);
    public static readonly Color BorderStrong = Color.FromArgb(96, 100, 116);
    public static readonly Color Accent = Color.FromArgb(122, 108, 240);
    public static readonly Color AccentSoft = Color.FromArgb(80, 122, 108, 240);
    public static readonly Color TextPrimary = Color.FromArgb(229, 231, 240);
    public static readonly Color TextMuted = Color.FromArgb(138, 143, 163);
    public static readonly Color TextDisabled = Color.FromArgb(86, 92, 112);
    public static readonly Color Danger = Color.FromArgb(232, 90, 102);
    public static readonly Color PulseWarn = Color.FromArgb(240, 176, 64);
    public static readonly Color Success = Color.FromArgb(82, 196, 138);

    // Button palettes (base / hover / pressed)
    public static readonly Color BtnPrimary = Color.FromArgb(108, 99, 220);
    public static readonly Color BtnPrimaryHover = Color.FromArgb(132, 122, 242);
    public static readonly Color BtnPrimaryPressed = Color.FromArgb(92, 84, 196);

    public static readonly Color BtnSuccess = Color.FromArgb(72, 168, 122);
    public static readonly Color BtnSuccessHover = Color.FromArgb(94, 192, 144);
    public static readonly Color BtnSuccessPressed = Color.FromArgb(58, 146, 104);

    public static readonly Color BtnWarning = Color.FromArgb(208, 142, 76);
    public static readonly Color BtnWarningHover = Color.FromArgb(228, 162, 96);
    public static readonly Color BtnWarningPressed = Color.FromArgb(184, 122, 60);

    public const int CornerRadius = 6;
}
