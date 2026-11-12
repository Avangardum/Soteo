namespace Soteo.Core.Entities.Units;

public sealed record EquipmentSlotState
{
    public required ItemStack? ItemStack { get; init; }
}
