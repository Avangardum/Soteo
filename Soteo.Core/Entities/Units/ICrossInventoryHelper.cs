namespace Soteo.Core.Entities.Units;

public interface ICrossInventoryHelper // todo inline into Unit maybe?
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
