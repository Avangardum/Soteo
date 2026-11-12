using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Dto.Commands;

public sealed record UnequipCommand(EquipmentSlot EquipmentSlot, int? ToInventorySlot = null) : ICommand;
