using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

public sealed record EquipCommand(int InventorySlot) : ICommand; // todo move commands to DTO folder
