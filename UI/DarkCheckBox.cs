using System.Drawing.Drawing2D;

namespace GodHandTrainer.UI;

internal sealed class DarkCheckBox : CheckBox
{
    private bool _hovered;
    private System.Windows.Forms.Timer? _pulseTimer;
    private float _pulsePhase;
    private bool _pulsing;

    public DarkCheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Standard;
        AutoSize = false;
        Height = 24;
        ForeColor = Theme.TextPrimary;
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Font = new Font("Segoe UI", 9.5f);
    }

    public void BeginPulse()
    {
        if (_pulsing) return;
        _pulsing = true;
        _pulsePhase = 0f;
        _pulseTimer ??= new System.Windows.Forms.Timer { Interval = 33 };
        _pulseTimer.Tick -= PulseTick;
        _pulseTimer.Tick += PulseTick;
        _pulseTimer.Start();
    }

    public void EndPulse()
    {
        _pulsing = false;
        _pulseTimer?.Stop();
        Invalidate();
    }

    private void PulseTick(object? sender, EventArgs e)
    {
        _pulsePhase += 0.22f;
        if (_pulsePhase > MathF.PI * 2f) _pulsePhase -= MathF.PI * 2f;
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _pulseTimer?.Stop();
            _pulseTimer?.Dispose();
            _pulseTimer = null;
        }
        base.Dispose(disposing);
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var textSize = TextRenderer.MeasureText(Text ?? string.Empty, Font);
        return new Size(textSize.Width + 30, Math.Max(24, textSize.Height + 6));
    }

    protected override void OnAutoSizeChanged(EventArgs e)
    {
        base.OnAutoSizeChanged(e);
        if (AutoSize) Size = GetPreferredSize(Size.Empty);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        if (AutoSize) Size = GetPreferredSize(Size.Empty);
        Invalidate();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        if (AutoSize) Size = GetPreferredSize(Size.Empty);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hovered = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Parent?.BackColor ?? Theme.Surface);

        const int boxSize = 16;
        var boxY = (Height - boxSize) / 2;
        var boxRect = new Rectangle(1, boxY, boxSize, boxSize);

        // Pulse t in [0,1] using a sine wave so the lerp eases smoothly.
        var pulseT = _pulsing ? (MathF.Sin(_pulsePhase) + 1f) * 0.5f : 0f;

        var boxFill = !Enabled ? Theme.Surface
            : Checked ? Theme.Accent
            : _hovered ? Theme.InputHover
            : Theme.Input;
        var boxBorder = !Enabled ? Theme.Border
            : Checked ? Theme.Accent
            : _hovered ? Theme.Accent
            : Theme.BorderStrong;

        if (_pulsing)
            boxBorder = Lerp(Theme.PulseWarn, Theme.Danger, pulseT);

        using (var path = RoundedRect(boxRect, 4))
        {
            using (var fill = new SolidBrush(boxFill))
                g.FillPath(fill, path);
            using (var pen = new Pen(boxBorder, _hovered || Checked || _pulsing ? 1.5f : 1f))
                g.DrawPath(pen, path);
        }

        if (Checked)
        {
            using var pen = new Pen(Color.White, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            var pts = new[]
            {
                new PointF(boxRect.Left + 3.5f, boxRect.Top + 8f),
                new PointF(boxRect.Left + 7f, boxRect.Top + 11f),
                new PointF(boxRect.Left + 12.5f, boxRect.Top + 5f),
            };
            g.DrawLines(pen, pts);
        }

        var textColor = !Enabled && !_pulsing ? Theme.TextDisabled
            : _pulsing ? Lerp(Theme.PulseWarn, Theme.Danger, pulseT)
            : _hovered ? Theme.TextPrimary
            : ForeColor;
        var textRect = new Rectangle(boxSize + 10, 0, Width - (boxSize + 10), Height);
        TextRenderer.DrawText(g, Text, Font, textRect, textColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        if (Focused && Enabled)
        {
            var ringRect = new Rectangle(boxRect.X - 2, boxRect.Y - 2, boxRect.Width + 4, boxRect.Height + 4);
            using var path = RoundedRect(ringRect, 6);
            using var pen = new Pen(Theme.AccentSoft, 1.5f);
            g.DrawPath(pen, path);
        }
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnCheckedChanged(EventArgs e)
    {
        base.OnCheckedChanged(e);
        Invalidate();
    }

    private static Color Lerp(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)(a.A + (b.A - a.A) * t),
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    private static GraphicsPath RoundedRect(Rectangle r, int radius)
    {
        var d = radius * 2;
        var path = new GraphicsPath();
        if (radius <= 0 || d > r.Width || d > r.Height) { path.AddRectangle(r); return path; }
        path.AddArc(r.X, r.Y, d, d, 180, 90);
        path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
