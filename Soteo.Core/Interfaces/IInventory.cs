using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;

namespace Soteo.Core.Interfaces;

public interface IInventory : IReadOnlyList<ItemStack?>
{
    bool HasSlot(int slot);

    /// <summary>
    /// Add a stack to the inventory. If inventory space is insufficient, the remainder is returned.
    /// </summary>
    ItemStack? Add(ItemStack stack);

    /// <summary>
    /// Put an item stack into a target slot. If the slot contains a stack that can be fully or partially merged with
    /// the put stack, it's merged and a remainder is returned. Otherwise, a stack that was in the slot previously is
    /// returned.
    /// </summary>
    ItemStack? PutAt(int slot, ItemStack stack);

    ItemStack? RemoveAt(int slot);
    IReadOnlyList<ItemStack?> ToSnapshot();
    void ReplicateSnapshot(IReadOnlyList<ItemStack?> stacks);
    void ApplyDelta(ListDelta<ItemStack?> delta);
}
