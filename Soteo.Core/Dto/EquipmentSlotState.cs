namespace Soteo.Core.Dto;

public sealed record EquipmentSlotState
{
    public required ItemStack? ItemStack { get; init; }
}
