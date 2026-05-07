using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace GodHandTrainer.UI;

internal sealed class DarkComboBox : ComboBox
{
    private const int WM_PAINT = 0x000F;
    private const int WM_ERASEBKGND = 0x0014;
    private const int WM_PRINTCLIENT = 0x0318;
    private const int WM_NCPAINT = 0x0085;
    private const int WM_CTLCOLORLISTBOX = 0x0134;

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string? pszSubAppName, string? pszSubIdList);

    [DllImport("user32.dll")]
    private static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport("user32.dll")]
    private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateSolidBrush(int crColor);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public Rectangle rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    private bool _hovered;

    public DarkComboBox()
    {
        DropDownStyle = ComboBoxStyle.DropDownList;
        FlatStyle = FlatStyle.Flat;
        DrawMode = DrawMode.OwnerDrawFixed;
        BackColor = Theme.Input;
        ForeColor = Theme.TextPrimary;
        Font = new Font("Segoe UI", 9.5f);
        ItemHeight = 22;

        SetStyle(ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ItemHeight = Math.Max(20, Font.Height + 6);
        // Strip Windows visual-style theming so the system stops drawing its white button overlay.
        SetWindowTheme(Handle, "", "");
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        ItemHeight = Math.Max(20, Font.Height + 6);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        BackColor = Enabled ? Theme.Input : Theme.Surface;
        ForeColor = Enabled ? Theme.TextPrimary : Theme.TextDisabled;
        Invalidate();
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

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnDropDown(EventArgs e)
    {
        base.OnDropDown(e);
        Invalidate();
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        Invalidate();
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0)
        {
            base.OnDrawItem(e);
            return;
        }

        var g = e.Graphics;
        var isDropdown = (e.State & DrawItemState.ComboBoxEdit) == 0;
        var selected = (e.State & DrawItemState.Selected) != 0 && isDropdown;

        var bg = !Enabled ? Theme.Surface
            : selected ? Theme.Accent
            : isDropdown ? Theme.Surface
            : Theme.Input;
        var fg = !Enabled ? Theme.TextDisabled
            : selected ? Color.White
            : Theme.TextPrimary;

        using (var b = new SolidBrush(bg)) g.FillRectangle(b, e.Bounds);

        var text = Items[e.Index]?.ToString() ?? string.Empty;
        var textRect = e.Bounds;
        textRect.X += isDropdown ? 10 : 8;
        textRect.Width -= isDropdown ? 12 : 10;

        TextRenderer.DrawText(g, text, e.Font ?? Font, textRect, fg,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_ERASEBKGND:
                m.Result = (IntPtr)1;
                return;

            case WM_NCPAINT:
                m.Result = IntPtr.Zero;
                return;

            case WM_PAINT:
            {
                var ps = new PAINTSTRUCT();
                BeginPaint(m.HWnd, ref ps);
                try
                {
                    using var g = Graphics.FromHdc(ps.hdc);
                    PaintAll(g);
                }
                finally { EndPaint(m.HWnd, ref ps); }
                m.Result = IntPtr.Zero;
                return;
            }

            case WM_PRINTCLIENT:
            {
                using var g = Graphics.FromHdc(m.WParam);
                PaintAll(g);
                m.Result = IntPtr.Zero;
                return;
            }

            case WM_CTLCOLORLISTBOX:
            {
                // Force the dropdown listbox background to our surface color.
                var brush = CreateSolidBrush(ColorToCOLORREF(Theme.Surface));
                m.Result = brush;
                return;
            }
        }

        base.WndProc(ref m);
    }

    private static int ColorToCOLORREF(Color c) => c.R | (c.G << 8) | (c.B << 16);

    private void PaintAll(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Clear with parent background so rounded corners blend.
        using (var clear = new SolidBrush(Parent?.BackColor ?? Theme.Background))
            g.FillRectangle(clear, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var bg = Enabled ? Theme.Input : Theme.Surface;

        using (var path = RoundedRect(rect, Theme.CornerRadius))
        {
            using (var fill = new SolidBrush(bg))
                g.FillPath(fill, path);

            var borderColor = !Enabled ? Theme.Border
                : Focused || DroppedDown ? Theme.Accent
                : _hovered ? Theme.BorderStrong
                : Theme.Border;
            using (var pen = new Pen(borderColor, (Focused || DroppedDown) ? 1.5f : 1f))
                g.DrawPath(pen, path);
        }

        // Right-side chevron pill — visually separates the click target.
        const int btnW = 28;
        var btnRect = new Rectangle(Width - btnW - 2, 3, btnW - 2, Height - 7);
        using (var path = RoundedRect(btnRect, Theme.CornerRadius - 2))
        {
            using var btnFill = new SolidBrush(Enabled ? Theme.InputButton : Theme.Surface);
            g.FillPath(btnFill, path);
        }

        // Subtle vertical divider just left of the pill.
        using (var sep = new Pen(Theme.Border))
            g.DrawLine(sep, btnRect.Left - 2, 5, btnRect.Left - 2, Height - 7);

        // Selected item text
        if (SelectedIndex >= 0)
        {
            var text = Items[SelectedIndex]?.ToString() ?? string.Empty;
            var textRect = new Rectangle(10, 0, Width - btnW - 14, Height);
            var fg = Enabled ? Theme.TextPrimary : Theme.TextDisabled;
            TextRenderer.DrawText(g, text, Font, textRect, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        // Filled chevron — chunky, modern.
        var arrowColor = Enabled ? Theme.TextPrimary : Theme.TextDisabled;
        using var brush = new SolidBrush(arrowColor);
        var cx = btnRect.Left + btnRect.Width / 2f;
        var cy = btnRect.Top + btnRect.Height / 2f;
        const float aw = 5f;
        const float ah = 3f;
        g.FillPolygon(brush, new[]
        {
            new PointF(cx - aw, cy - ah / 2f),
            new PointF(cx + aw, cy - ah / 2f),
            new PointF(cx, cy + ah + 0.5f),
        });
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
