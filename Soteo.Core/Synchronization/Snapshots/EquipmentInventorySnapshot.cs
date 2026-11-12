namespace Soteo.Core.Synchronization.Snapshots;

public sealed record EquipmentInventorySnapshot(IReadOnlyDictionary<EquipmentSlot, ItemStack?> Stacks);
