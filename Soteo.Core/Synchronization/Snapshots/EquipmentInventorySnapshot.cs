using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Snapshots;

public sealed record EquipmentInventorySnapshot(IReadOnlyDictionary<EquipmentSlot, ItemStack?> Stacks);
