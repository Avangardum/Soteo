namespace Soteo.Core.Snapshots;

public sealed record EquipmentInventorySnapshot(IReadOnlyDictionary<EquipmentSlot, ItemStack?> Stacks);
