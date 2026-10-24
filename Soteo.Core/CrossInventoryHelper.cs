using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core;

public sealed class CrossInventoryHelper : ICrossInventoryHelper
{
    public bool Equip
    (
        IInventory inventory,
        IEquipmentInventory equipmentInventory,
        int fromInventorySlot,
        EquipmentSlot? toEquipmentSlot
    )
    {
        if (!inventory.HasSlot(fromInventorySlot)) return false;
        ItemStack? inventoryStack = inventory[fromInventorySlot];
        if (inventoryStack?.Item.EquippableTo == null) return false;
        if (!equipmentInventory.ContainsKey(inventoryStack.Item.EquippableTo.Value)) return false;
        if (toEquipmentSlot != null && toEquipmentSlot != inventoryStack.Item.EquippableTo) return false;

        inventory.RemoveAt(fromInventorySlot);
        ItemStack? remainder = equipmentInventory.PutAt(inventoryStack.Item.EquippableTo.Value, inventoryStack);
        if (remainder != null)
            inventory.PutAt(fromInventorySlot, remainder);
        return true;
    }

    public bool Unequip
    (
        IInventory inventory,
        IEquipmentInventory equipmentInventory,
        EquipmentSlot fromEquipmentSlot,
        int? toInventorySlot
    )
    {
        if (!equipmentInventory.TryGetValue(fromEquipmentSlot, out ItemStack? equipmentStack)) return false;
        if (equipmentStack == null) return false;
        if (toInventorySlot != null)
        {
            if (!inventory.HasSlot(toInventorySlot.Value)) return false;
            ItemStack? inventoryStack = inventory[toInventorySlot.Value];
            if (inventoryStack != null)
            {
                if (inventoryStack.Item != equipmentStack.Item) return false;
                if (inventoryStack.FreeSpace == 0) return false;
            }
        }

        equipmentInventory.RemoveAt(fromEquipmentSlot);
        ItemStack? remainder = toInventorySlot == null ?
            inventory.Add(equipmentStack) :
            inventory.PutAt(toInventorySlot.Value, equipmentStack);
        if (remainder != null)
            equipmentInventory.PutAt(fromEquipmentSlot, remainder);
        return true;
    }
}
