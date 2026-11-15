namespace Soteo.Core.Entities.Units;

public interface ICrossInventoryHelper
{
    bool Equip
    (
        Inventory inventory,
        IEquipmentInventory equipmentInventory,
        int fromInventorySlot,
        EquipmentSlot? toEquipmentSlot
    );

    bool Unequip
    (
        Inventory inventory,
        IEquipmentInventory equipmentInventory,
        EquipmentSlot fromEquipmentSlot,
        int? toInventorySlot
    );
}
