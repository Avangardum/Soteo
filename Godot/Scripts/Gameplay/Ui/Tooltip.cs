using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Ui;

public sealed class Tooltip : Control, ITooltip
{
    private Label _headerLabel = null!;
    private RichTextLabel _bodyLabel = null!;

    public override void _Ready()
    {
        _headerLabel = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/Header");
        _bodyLabel = GetNode<RichTextLabel>("PanelContainer/MarginContainer/VBoxContainer/Body");
    }

    public string Header
    {
        get => _headerLabel.Text;
        set => _headerLabel.Text = value;
    }

    public string Body
    {
        get => _bodyLabel.Text;
        set => _bodyLabel.Text = value;
    }
}