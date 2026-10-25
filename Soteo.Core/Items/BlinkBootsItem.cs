using Soteo.Core.Enums;

namespace Soteo.Core.Items;

public sealed class BlinkBootsItem : Item
{
    public override EquipmentSlot? EquippableTo => EquipmentSlot.Feet;
    public override IReadOnlyList<EquipmentSlot> BlocksEquipmentSlots => [EquipmentSlot.Potion];
}
