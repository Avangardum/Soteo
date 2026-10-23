using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;

namespace Soteo.Core;

public sealed class EquipmentInventory
{
    private Dictionary<EquipmentSlot, ItemStack?> _stacks;

    public EquipmentInventory(params IReadOnlyList<EquipmentSlot> slots)
    {
        _stacks = slots.ToDictionary(it => it, ItemStack? (it) => null);
    }

    public EquipmentInventory(IReadOnlyDictionary<EquipmentSlot, ItemStack?> stacks)
    {
        _stacks = stacks.ToDictionary();
    }

    public ItemStack? this[EquipmentSlot slot] => _stacks[slot];

    public bool HasSlot(EquipmentSlot slot) => _stacks.ContainsKey(slot);

    /// <summary>
    /// Put an item stack into a target slot. If the slot contains a stack that can be fully or partially merged with
    /// the put stack, it's merged and a remainder is returned. If the slot contains a stack that can't be merged,
    /// nothing is changed and the input stack is returned.
    /// </summary>
    public ItemStack? PutAt(EquipmentSlot slot, ItemStack stack)
    {
        if (stack.Item.EquippableTo != slot)
            return stack;

        ItemStack? existingStack = _stacks[slot];

        if (existingStack == null)
        {
            _stacks[slot] = stack;
            return null;
        }

        if (existingStack.Item != stack.Item || existingStack.FreeSpace == 0)
            return stack;

        long movedCount = Math.Min(existingStack.FreeSpace, stack.Count);
        _stacks[slot] = existingStack.WithCount(existingStack.Count + movedCount);
        return stack.WithCount(stack.Count - movedCount);
    }

    public EquipmentInventorySnapshot ToSnapshot() => new(_stacks.ToDictionary());

    public void ReplicateSnapshot(EquipmentInventorySnapshot snapshot)
    {
        _stacks = snapshot.Stacks.ToDictionary();
    }

    public void ApplyDelta(EquipmentInventoryDelta delta)
    {
        delta.Stacks.MutateDictionary(_stacks);
    }
}
