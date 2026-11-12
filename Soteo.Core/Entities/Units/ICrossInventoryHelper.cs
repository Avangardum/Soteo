namespace Soteo.Core.Entities.Units;

public interface ICrossInventoryHelper
{
    bool Equip
    (
        IInventory inventory,
        IEquipmentInventory equipmentInventory,
        int fromInventorySlot,
        EquipmentSlot? toEquipmentSlot
    );

    bool Unequip
    (
        IInventory inventory,
        IEquipmentInventory equipmentInventory,
        EquipmentSlot fromEquipmentSlot,
        int? toInventorySlot
    );
}
