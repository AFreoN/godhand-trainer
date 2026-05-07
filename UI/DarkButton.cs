using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace GodHandTrainer.UI;

internal enum DarkButtonStyle
{
    Primary,
    Success,
    Warning,
}

internal sealed class DarkButton : Button
{
    private bool _hovered;
    private bool _pressed;
    private DarkButtonStyle _style = DarkButtonStyle.Primary;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DarkButtonStyle Style
    {
        get => _style;
        set { _style = value; Invalidate(); }
    }

    public DarkButton()
    {
        SetStyle(ControlStyles.UserPaint
                 | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw
                 | ControlStyles.SupportsTransparentBackColor, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = Color.Transparent;
        ForeColor = Color.White;
        Font = new Font("Segoe UI Semibold", 9f);
        Cursor = Cursors.Hand;
        Height = 30;
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
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        if (mevent.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _pressed = false;
        Invalidate();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        Cursor = Enabled ? Cursors.Hand : Cursors.Default;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var clear = new SolidBrush(Parent?.BackColor ?? Theme.Surface))
            g.FillRectangle(clear, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var (baseColor, hover, pressed) = _style switch
        {
            DarkButtonStyle.Success => (Theme.BtnSuccess, Theme.BtnSuccessHover, Theme.BtnSuccessPressed),
            DarkButtonStyle.Warning => (Theme.BtnWarning, Theme.BtnWarningHover, Theme.BtnWarningPressed),
            _ => (Theme.BtnPrimary, Theme.BtnPrimaryHover, Theme.BtnPrimaryPressed),
        };

        Color fill;
        if (!Enabled) fill = Theme.Surface;
        else if (_pressed) fill = pressed;
        else if (_hovered) fill = hover;
        else fill = baseColor;

        using (var path = RoundedRect(rect, Theme.CornerRadius))
        {
            using (var brush = new SolidBrush(fill))
                g.FillPath(brush, path);

            // Top highlight glow on hover for that "lifted" feel.
            if (_hovered && Enabled && !_pressed)
            {
                using var glow = new LinearGradientBrush(
                    new Rectangle(0, 0, Width, Height / 2),
                    Color.FromArgb(40, 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                    LinearGradientMode.Vertical);
                using var glowPath = RoundedRect(new Rectangle(0, 0, Width - 1, Height / 2), Theme.CornerRadius);
                g.FillPath(glow, glowPath);
            }
        }

        var fg = Enabled ? ForeColor : Theme.TextDisabled;
        TextRenderer.DrawText(g, Text, Font, ClientRectangle, fg,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

        if (Focused && Enabled)
        {
            var ring = new Rectangle(2, 2, Width - 5, Height - 5);
            using var path = RoundedRect(ring, Theme.CornerRadius - 1);
            using var pen = new Pen(Color.FromArgb(120, 255, 255, 255), 1f) { DashStyle = DashStyle.Dot };
            g.DrawPath(pen, path);
        }
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
