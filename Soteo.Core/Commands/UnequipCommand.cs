namespace Soteo.Core.Commands;

public sealed record UnequipCommand(EquipmentSlot EquipmentSlot, int? ToInventorySlot = null) : Command;
