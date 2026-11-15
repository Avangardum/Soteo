namespace Soteo.Core.Entities.Units;

public interface ICrossInventoryHelper
{
    bool Equip
    (
        Inventory inventory,
        EquipmentInventory equipmentInventory,
        int fromInventorySlot,
        EquipmentSlot? toEquipmentSlot
    );

    bool Unequip
    (
        Inventory inventory,
        EquipmentInventory equipmentInventory,
        EquipmentSlot fromEquipmentSlot,
        int? toInventorySlot
    );
}
