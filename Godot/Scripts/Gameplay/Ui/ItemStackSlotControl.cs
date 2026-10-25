using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class ItemStackSlotControl : Control
{
    private readonly LateInit<ItemStackControl> _itemStack = new();
    private readonly LateInit<Control> _shade = new();

    public override void _Ready()
    {
        _itemStack.Value = GetNode<ItemStackControl>("ItemStack");
        _shade.Value = GetNode<Control>("Shade");
    }

    public ItemStackControl ItemStack => _itemStack.Value;
    public Control Shade => _shade.Value;
}
