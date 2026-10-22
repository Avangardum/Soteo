using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

public sealed record UnequipCommand(EquipmentSlot EquipmentSlot, int? ToInventorySlot = null) : ICommand;
