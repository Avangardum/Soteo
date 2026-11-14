namespace Soteo.Core.Synchronization.Deltas;

public sealed record InventoryDelta(ListDelta<ItemStack?> Stacks)
{
    public static InventoryDelta FromNew(InventorySnapshot snapshot)
    {
        return new(ListDelta.FromNew(snapshot.Stacks));
    }

    public static InventoryDelta Between(InventorySnapshot from, InventorySnapshot to)
    {
        return new(ListDelta.Between(from.Stacks, to.Stacks));
    }

    public bool HasChanged => Stacks.HasChanged;
}
