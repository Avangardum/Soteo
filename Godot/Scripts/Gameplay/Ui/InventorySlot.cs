using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class InventorySlot : Control
{
    private readonly LateInit<TextureRect> _textureRect = new();
    private readonly LateInit<Control> _countBackground = new();
    private readonly LateInit<Label> _countLabel = new();

    public override void _Ready()
    {
        _textureRect.Value = GetNode<TextureRect>("TextureRect");
        _countBackground.Value = GetNode<Control>("TextureRect/CountBackground");
        _countLabel.Value = GetNode<Label>("TextureRect/CountBackground/CountLabel");
    }

    public TextureRect TextureRect => _textureRect.Value;
    public Control CountBackground => _countBackground.Value;
    public Label CountLabel => _countLabel.Value;
}
