using System.Collections;
using System.Collections.Immutable;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;

namespace Soteo.Core;

public sealed class EquipmentInventory : IEquipmentInventory
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
        if (stack.Item.EquippableTo != slot) return stack;
        if (BlockedSlots.Contains(slot)) return stack;

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

    public ItemStack? RemoveAt(EquipmentSlot slot)
    {
        ItemStack? result = _stacks[slot];
        _stacks[slot] = null;
        return result;
    }

    public IReadOnlyList<EquipmentSlot> BlockedSlots =>
        _stacks.Values.WhereNotNull().SelectMany(it => it.Item.BlocksEquipmentSlots).Distinct().ToImmutableList();

    public EquipmentInventorySnapshot ToSnapshot() => new(_stacks.ToDictionary());

    public void ReplicateSnapshot(EquipmentInventorySnapshot snapshot)
    {
        _stacks = snapshot.Stacks.ToDictionary();
    }

    public void ApplyDelta(EquipmentInventoryDelta delta)
    {
        delta.Stacks.MutateDictionary(_stacks);
    }

    public IEnumerator<KeyValuePair<EquipmentSlot, ItemStack?>> GetEnumerator() => _stacks.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _stacks.Count;

    public bool ContainsKey(EquipmentSlot key) => _stacks.ContainsKey(key);

    public bool TryGetValue(EquipmentSlot key, out ItemStack? value) => _stacks.TryGetValue(key, out value);

    public IEnumerable<EquipmentSlot> Keys => _stacks.Keys;

    public IEnumerable<ItemStack?> Values => _stacks.Values;
}
