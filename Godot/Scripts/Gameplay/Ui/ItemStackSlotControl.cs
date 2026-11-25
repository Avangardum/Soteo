namespace Soteo.Main.Gameplay.Ui;

public sealed partial class ItemStackSlotControl : Control
{
    public ItemStackControl ItemStack { get; private set; } = null!;

    public override void _Ready()
    {
        ItemStack = GetNode<ItemStackControl>("ItemStack");
    }
}
