using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class InventorySlot : Control
{
    private readonly LateInit<TextureRect> _textureRect = new();
    private readonly LateInit<Control> _countBackground = new();
    private readonly LateInit<Label> _countLabel = new();
    private readonly LateInit<ItemStackControl> _itemStack = new();
    private readonly LateInit<Control> _dropArea = new();

    public event Action DragStarted
    {
        add => _itemStack.Value.DragStarted += value;
        remove => _itemStack.Value.DragStarted -= value;
    }

    public event Action DragStopped
    {
        add => _itemStack.Value.DragStopped += value;
        remove => _itemStack.Value.DragStopped -= value;
    }

    public override void _Ready()
    {
        _textureRect.Value = GetNode<TextureRect>("ItemStack/TextureRect");
        _countBackground.Value = GetNode<Control>("ItemStack/TextureRect/CountBackground");
        _countLabel.Value = GetNode<Label>("ItemStack/TextureRect/CountBackground/CountLabel");
        _itemStack.Value = GetNode<ItemStackControl>("ItemStack");
        _dropArea.Value = GetNode<Control>("DropArea");
    }

    public TextureRect TextureRect => _textureRect.Value;
    public Control CountBackground => _countBackground.Value;
    public Label CountLabel => _countLabel.Value;
    public Control DropArea => _dropArea.Value;
}
