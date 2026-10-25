using Soteo.Core.Interfaces;

namespace Soteo.Core.Dto.Commands;

public sealed record EquipCommand(int InventorySlot) : ICommand;
