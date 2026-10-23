using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Deltas;

public sealed record EquipmentInventoryDelta(DictionaryDelta<EquipmentSlot, ItemStack?> Stacks);
