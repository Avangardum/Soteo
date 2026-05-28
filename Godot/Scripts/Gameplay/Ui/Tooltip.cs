using Soteo.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Gameplay.Ui;

public sealed class Tooltip : Control, ITooltip
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
        
        _bodyDefaultMinWidth.Value = BodyLabel.RectMinSize.x;
    }

    public void Show(Vector2 position, string header, string body)
    {
        Visible = true;
        RectGlobalPosition = position.ToGd();
        HeaderLabel.Text = header;
        BodyLabel.BbcodeText = body;
        HeaderLabel.Visible = header != "";
        BodyLabel.Visible = body != "";
        Separator.Visible = header != "" && body != "";
        UpdateBodyMinWidth();
    }
    
    private void UpdateBodyMinWidth()
    {
        BodyLabel.RectMinSize = new GdVector2(_bodyDefaultMinWidth, 0);
        BodyLabel.RectSize = BodyLabel.RectSize with { x = _bodyDefaultMinWidth };
        
        string text = BodyLabel.BbcodeText;
        BodyLabel.BbcodeText = "Lorem ipsum";
        int lineHeight = BodyLabel.GetContentHeight();
        BodyLabel.BbcodeText = text;
        
        const int step = 10; 
        while (BodyLabel.GetContentHeight() == lineHeight && BodyLabel.RectMinSize.x > step)
        {
            BodyLabel.RectMinSize -= new GdVector2(step, 0);
            BodyLabel.RectSize = BodyLabel.RectSize with { x = BodyLabel.RectMinSize.x };
        }
        BodyLabel.RectMinSize += new GdVector2(step, 0);
    }
    
    public new void Hide() => Visible = false;
}
