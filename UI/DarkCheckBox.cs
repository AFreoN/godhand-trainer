using System.Drawing.Drawing2D;

namespace GodHandTrainer.UI;

internal sealed class DarkCheckBox : CheckBox
{
    private static readonly Color DisabledFore = Color.FromArgb(120, 120, 124);
    private static readonly Color BoxBorder = Color.FromArgb(140, 140, 148);
    private static readonly Color BoxFill = Color.FromArgb(48, 48, 54);
    private static readonly Color CheckMark = Color.FromArgb(110, 170, 240);

    public DarkCheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        FlatStyle = FlatStyle.Standard;
        AutoSize = false;
        Height = 22;
    }

    public override Size GetPreferredSize(Size proposedSize)
    {
        var textSize = TextRenderer.MeasureText(Text ?? string.Empty, Font);
        return new Size(textSize.Width + 28, Math.Max(22, textSize.Height + 4));
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

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(BackColor);

        const int boxSize = 14;
        var boxY = (Height - boxSize) / 2;
        var boxRect = new Rectangle(1, boxY, boxSize, boxSize);

        using (var fill = new SolidBrush(BoxFill)) g.FillRectangle(fill, boxRect);
        using (var pen = new Pen(BoxBorder)) g.DrawRectangle(pen, boxRect);

        if (Checked)
        {
            using var pen = new Pen(CheckMark, 2f);
            var pts = new[]
            {
                new Point(boxRect.Left + 3, boxRect.Top + 7),
                new Point(boxRect.Left + 6, boxRect.Top + 10),
                new Point(boxRect.Left + 11, boxRect.Top + 4),
            };
            g.DrawLines(pen, pts);
        }

        var textColor = Enabled ? ForeColor : DisabledFore;
        var textRect = new Rectangle(boxSize + 8, 0, Width - (boxSize + 8), Height);
        TextRenderer.DrawText(g, Text, Font, textRect, textColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        if (Focused)
        {
            using var pen = new Pen(Color.FromArgb(80, CheckMark));
            pen.DashStyle = DashStyle.Dot;
            g.DrawRectangle(pen, textRect.X - 1, 1, textRect.Width - 2, Height - 3);
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
}
