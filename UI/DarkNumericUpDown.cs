using System.Drawing.Drawing2D;

namespace GodHandTrainer.UI;

internal sealed class DarkNumericUpDown : NumericUpDown
{
    private readonly Control? _upDownButtons;
    private readonly TextBox? _textBox;
    private SpinnerHook? _spinnerHook;
    private bool _hovered;
    private int _hotButton;

    private const int SpinnerWidth = 22;
    private const int PadX = 10;

    public DarkNumericUpDown()
    {
        BorderStyle = BorderStyle.None;
        BackColor = Theme.Input;
        ForeColor = Theme.TextPrimary;
        Font = new Font("Segoe UI", 9.5f);
        Height = 30;
        Margin = new Padding(0);

        foreach (Control c in Controls)
        {
            switch (c)
            {
                case TextBox tb:
                    _textBox = tb;
                    tb.BackColor = Theme.Input;
                    tb.ForeColor = Theme.TextPrimary;
                    tb.BorderStyle = BorderStyle.None;
                    break;
                default:
                    if (c.GetType().Name == "UpDownButtons")
                    {
                        _upDownButtons = c;
                        c.MouseMove += UpDownMouseMove;
                        c.MouseLeave += UpDownMouseLeave;
                        _spinnerHook = new SpinnerHook(this, c);
                    }
                    break;
            }
        }

        SetStyle(ControlStyles.UserPaint
                 | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.OptimizedDoubleBuffer
                 | ControlStyles.ResizeRedraw, true);
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        LayoutInner();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        LayoutInner();
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);
        var bg = Enabled ? Theme.Input : Theme.Surface;
        var fg = Enabled ? Theme.TextPrimary : Theme.TextDisabled;
        BackColor = bg;
        ForeColor = fg;
        if (_textBox is not null)
        {
            _textBox.BackColor = bg;
            _textBox.ForeColor = fg;
        }
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

    private void LayoutInner()
    {
        if (_textBox is not null)
        {
            var tbHeight = _textBox.PreferredHeight;
            var top = (Height - tbHeight) / 2;
            var width = Width - SpinnerWidth - PadX - 6;
            if (width < 10) width = 10;
            _textBox.SetBounds(PadX, top, width, tbHeight);
        }
        if (_upDownButtons is not null)
        {
            _upDownButtons.SetBounds(Width - SpinnerWidth - 2, 2, SpinnerWidth, Height - 4);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var bg = Enabled ? Theme.Input : Theme.Surface;
        using (var bgBrush = new SolidBrush(Parent?.BackColor ?? Theme.Background))
            g.FillRectangle(bgBrush, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using (var path = RoundedRect(rect, Theme.CornerRadius))
        {
            using (var fill = new SolidBrush(bg))
                g.FillPath(fill, path);

            var borderColor = !Enabled ? Theme.Border
                : Focused ? Theme.Accent
                : _hovered ? Theme.BorderStrong
                : Theme.Border;
            using (var pen = new Pen(borderColor, Focused ? 1.5f : 1f))
                g.DrawPath(pen, path);
        }

        _textBox?.Invalidate();
        _upDownButtons?.Invalidate();
    }

    private void UpDownMouseMove(object? sender, MouseEventArgs e)
    {
        if (sender is not Control c) return;
        var hot = e.Y < c.Height / 2 ? 1 : 2;
        if (hot != _hotButton)
        {
            _hotButton = hot;
            c.Invalidate();
        }
    }

    private void UpDownMouseLeave(object? sender, EventArgs e)
    {
        _hotButton = 0;
        if (sender is Control c) c.Invalidate();
    }

    private void RenderSpinner(Graphics g, Control c)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (var bg = new SolidBrush(Enabled ? Theme.Input : Theme.Surface))
            g.FillRectangle(bg, c.ClientRectangle);

        var w = c.Width;
        var h = c.Height;
        var midY = h / 2;

        // Subtle vertical separator from text area.
        using (var sep = new Pen(Theme.Border))
            g.DrawLine(sep, 0, 3, 0, h - 4);

        // Hot states.
        if (Enabled && _hotButton != 0)
        {
            using var hot = new SolidBrush(Theme.InputHover);
            var hotRect = _hotButton == 1
                ? new Rectangle(1, 1, w - 1, midY - 1)
                : new Rectangle(1, midY, w - 1, h - midY - 1);
            using var path = RoundedRect(hotRect, 3);
            g.FillPath(hot, path);
        }

        // Filled triangle arrows — modern, chunkier.
        var arrowColor = Enabled ? Theme.TextPrimary : Theme.TextDisabled;
        using var brush = new SolidBrush(arrowColor);

        var cx = w / 2f;
        var upY = midY * 0.5f + 1f;
        var dnY = midY + (h - midY) * 0.5f - 1f;
        const float aw = 4.5f;
        const float ah = 3f;

        g.FillPolygon(brush, new[]
        {
            new PointF(cx - aw, upY + ah / 2f),
            new PointF(cx + aw, upY + ah / 2f),
            new PointF(cx, upY - ah),
        });
        g.FillPolygon(brush, new[]
        {
            new PointF(cx - aw, dnY - ah / 2f),
            new PointF(cx + aw, dnY - ah / 2f),
            new PointF(cx, dnY + ah),
        });
    }

    private sealed class SpinnerHook : NativeWindow
    {
        private const int WM_PAINT = 0x000F;
        private const int WM_ERASEBKGND = 0x0014;
        private const int WM_PRINTCLIENT = 0x0318;

        private readonly DarkNumericUpDown _owner;
        private readonly Control _spinner;

        public SpinnerHook(DarkNumericUpDown owner, Control spinner)
        {
            _owner = owner;
            _spinner = spinner;
            if (spinner.IsHandleCreated) AssignHandle(spinner.Handle);
            else spinner.HandleCreated += (_, _) => AssignHandle(spinner.Handle);
            spinner.HandleDestroyed += (_, _) => ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_ERASEBKGND:
                    m.Result = (IntPtr)1;
                    return;
                case WM_PAINT:
                {
                    var ps = new PAINTSTRUCT();
                    var hdc = BeginPaint(m.HWnd, ref ps);
                    try
                    {
                        using var g = Graphics.FromHdc(hdc);
                        _owner.RenderSpinner(g, _spinner);
                    }
                    finally { EndPaint(m.HWnd, ref ps); }
                    m.Result = IntPtr.Zero;
                    return;
                }
                case WM_PRINTCLIENT:
                {
                    using var g = Graphics.FromHdc(m.WParam);
                    _owner.RenderSpinner(g, _spinner);
                    m.Result = IntPtr.Zero;
                    return;
                }
            }
            base.WndProc(ref m);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public System.Drawing.Rectangle rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
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
