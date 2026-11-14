namespace Soteo.Core.Synchronization.Snapshots;

public sealed record InventorySnapshot(IReadOnlyList<ItemStack?> Stacks);
