using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed class InventorySlotControl : Control // todo rename
{
    private readonly LateInit<ItemStackControl> _itemStack = new();

    public override void _Ready()
    {
        _itemStack.Value = GetNode<ItemStackControl>("ItemStack");
    }

    public ItemStackControl ItemStack => _itemStack.Value;
}
