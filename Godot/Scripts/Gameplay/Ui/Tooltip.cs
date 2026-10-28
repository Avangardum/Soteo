using Soteo.Main.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed partial class Tooltip : Control, ITooltip
{
    private readonly LateInit<Label> _headerLabel = new();
    private readonly LateInit<HSeparator> _separator = new();
    private readonly LateInit<RichTextLabel> _bodyLabel = new();
    private readonly LateInit<float> _bodyDefaultMinWidth = new();

    public Label HeaderLabel => _headerLabel;
    public HSeparator Separator => _separator;
    public RichTextLabel BodyLabel => _bodyLabel;

    public override void _Ready()
    {
        _headerLabel.Value = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Header");
        _separator.Value = GetNode<HSeparator>("PanelContainer/MarginContainer/VBoxContainer/HSeparator");
        _bodyLabel.Value = GetNode<RichTextLabel>("PanelContainer/MarginContainer/VBoxContainer/Body");

        _bodyDefaultMinWidth.Value = BodyLabel.CustomMinimumSize.X;
    }

    public void Show(Vector2 position, string header, string body)
    {
        Visible = true;
        GlobalPosition = position.ToGd();
        HeaderLabel.Text = header;
        BodyLabel.Text = body;
        HeaderLabel.Visible = header != "";
        BodyLabel.Visible = body != "";
        Separator.Visible = header != "" && body != "";
        UpdateBodyMinWidth();
    }

    private void UpdateBodyMinWidth()
    {
        BodyLabel.CustomMinimumSize = new GdVector2(_bodyDefaultMinWidth, 0);
        BodyLabel.Size = BodyLabel.Size with { X = _bodyDefaultMinWidth };

        string text = BodyLabel.Text;
        BodyLabel.Text = "Lorem ipsum";
        int lineHeight = BodyLabel.GetContentHeight();
        BodyLabel.Text = text;

        const int step = 10;
        while (BodyLabel.GetContentHeight() == lineHeight && BodyLabel.CustomMinimumSize.X > step)
        {
            BodyLabel.CustomMinimumSize -= new GdVector2(step, 0);
            BodyLabel.Size = BodyLabel.Size with { X = BodyLabel.CustomMinimumSize.X };
        }
        BodyLabel.CustomMinimumSize += new GdVector2(step, 0);
    }

    public new void Hide() => Visible = false;
}
