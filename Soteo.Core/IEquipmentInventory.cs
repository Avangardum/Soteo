using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;

namespace Soteo.Core;

public interface IEquipmentInventory : IReadOnlyDictionary<EquipmentSlot, ItemStack?>
{
    /// <summary>
    /// Put an item stack into a target slot. If the slot contains a stack that can be fully or partially merged with
    /// the put stack, it's merged and a remainder is returned. If the slot contains a stack that can't be merged,
    /// nothing is changed and the input stack is returned.
    /// </summary>
    ItemStack? PutAt(EquipmentSlot slot, ItemStack stack);

    ItemStack? RemoveAt(EquipmentSlot slot);

    /// <summary>
    /// Slots that are empty, but blocked by items in other slots as keys and slots that block them as values
    /// </summary>
    IReadOnlyDictionary<EquipmentSlot, EquipmentSlot> BlockedAndBlockingSlots { get; }

    EquipmentInventorySnapshot ToSnapshot();

    void ReplicateSnapshot(EquipmentInventorySnapshot snapshot);

    void ApplyDelta(EquipmentInventoryDelta delta);
}
