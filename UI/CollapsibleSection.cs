using System.ComponentModel;

namespace GodHandTrainer.UI;

internal sealed class CollapsibleSection : Panel
{
    private readonly Label _header;
    private readonly Panel _content;
    private bool _expanded = true;
    private int _expandedContentHeight;

    public event EventHandler? StateChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Panel Content => _content;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        get => _header.Text;
        set => _header.Text = FormatTitle(value);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Expanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value) return;
            _expanded = value;
            ApplyState();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int ExpandedContentHeight
    {
        get => _expandedContentHeight;
        set
        {
            _expandedContentHeight = value;
            if (_expanded) Height = HeaderHeight + value;
            _content.Height = value;
        }
    }

    public int HeaderHeight => 28;

    public CollapsibleSection()
    {
        BackColor = Color.FromArgb(38, 38, 44);

        _header = new Label
        {
            Dock = DockStyle.Top,
            Height = HeaderHeight,
            BackColor = Color.FromArgb(46, 46, 54),
            ForeColor = Color.Gainsboro,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            Font = new Font("Segoe UI Semibold", 9.5f),
            Cursor = Cursors.Hand,
        };
        _header.Click += (_, _) => Expanded = !_expanded;
        Controls.Add(_header);

        _content = new Panel
        {
            Location = new Point(0, HeaderHeight),
            BackColor = Color.FromArgb(38, 38, 44),
            AutoScroll = true,
        };
        Controls.Add(_content);

        Title = "Section";
        ApplyState();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        _content.Width = Width - SystemInformation.VerticalScrollBarWidth;
        if (_expanded) _content.Height = Height - HeaderHeight;
    }

    private void ApplyState()
    {
        _header.Text = FormatTitle(StripChevron(_header.Text));
        _content.Visible = _expanded;
        Height = _expanded ? HeaderHeight + _expandedContentHeight : HeaderHeight;
    }

    private string FormatTitle(string title)
    {
        var chev = _expanded ? "▼" : "▶";
        return $" {chev}  {StripChevron(title)}";
    }

    private static string StripChevron(string text)
    {
        var t = text?.TrimStart() ?? string.Empty;
        if (t.StartsWith("▼") || t.StartsWith("▶"))
            t = t.Substring(1).TrimStart();
        return t;
    }
}
