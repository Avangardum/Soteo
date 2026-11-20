namespace Soteo.Core.Snapshots;

public sealed record InventorySnapshot(IReadOnlyList<ItemStack?> Stacks);
