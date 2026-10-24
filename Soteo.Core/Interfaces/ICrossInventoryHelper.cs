using Soteo.Core.Enums;

namespace Soteo.Core.Interfaces;

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
