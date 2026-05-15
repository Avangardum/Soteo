using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Ui;

public sealed class Tooltip : Control, ITooltip
{
    private Label _headerLabel = null!;
    private HSeparator _separator = null!;
    private RichTextLabel _bodyLabel = null!;
    
    private float _bodyDefaultMinWidth;

    public override void _Ready()
    {
        _headerLabel = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Header");
        _separator = GetNode<HSeparator>("PanelContainer/MarginContainer/VBoxContainer/HSeparator");
        _bodyLabel = GetNode<RichTextLabel>("PanelContainer/MarginContainer/VBoxContainer/Body");
        
        _bodyDefaultMinWidth = _bodyLabel.RectMinSize.x;
    }

    public void Show(Vector2 position, string header, string body)
    {
        Visible = true;
        RectPosition = position;
        _headerLabel.Text = header;
        _bodyLabel.BbcodeText = body;
        _headerLabel.Visible = header != "";
        _bodyLabel.Visible = body != "";
        _separator.Visible = header != "" && body != "";
        UpdateBodyMinWidth();
    }
    
    private void UpdateBodyMinWidth()
    {
        _bodyLabel.RectMinSize = new Vector2(_bodyDefaultMinWidth, 0);
        _bodyLabel.RectSize = _bodyLabel.RectSize with { x = _bodyDefaultMinWidth };
        
        string text = _bodyLabel.BbcodeText;
        _bodyLabel.BbcodeText = "Lorem ipsum";
        int lineHeight = _bodyLabel.GetContentHeight();
        _bodyLabel.BbcodeText = text;
        
        const int step = 10; 
        while (_bodyLabel.GetContentHeight() == lineHeight && _bodyLabel.RectMinSize.x > step)
        {
            _bodyLabel.RectMinSize -= new Vector2(step, 0);
            _bodyLabel.RectSize = _bodyLabel.RectSize with { x = _bodyLabel.RectMinSize.x };
        }
        _bodyLabel.RectMinSize += new Vector2(step, 0);
    }
    
    public new void Hide() => Visible = false;
}