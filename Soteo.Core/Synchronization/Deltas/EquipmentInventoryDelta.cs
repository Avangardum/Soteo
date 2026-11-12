namespace Soteo.Core.Synchronization.Deltas;

public sealed record EquipmentInventoryDelta(DictionaryDelta<EquipmentSlot, ItemStack?> Stacks)
{
    public static EquipmentInventoryDelta FromNew(EquipmentInventorySnapshot snapshot)
    {
        return new(DictionaryDelta.FromNew(snapshot.Stacks));
    }

    public static EquipmentInventoryDelta Between(EquipmentInventorySnapshot from, EquipmentInventorySnapshot to)
    {
        return new(DictionaryDelta.Between(from.Stacks, to.Stacks));
    }

    public bool HasChanged => Stacks.HasChanged;
}
