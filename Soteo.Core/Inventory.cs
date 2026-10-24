using System.Collections;
using System.Collections.Immutable;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;

namespace Soteo.Core;

public sealed class Inventory : IReadOnlyList<ItemStack?>
{
    private List<ItemStack?> _stacks;

    public Inventory(int size)
    {
        _stacks = Enumerable.Repeat<ItemStack?>(null, size).ToList();
    }

    public Inventory(IReadOnlyList<ItemStack?> stacks)
    {
        _stacks = stacks.ToList();
    }

    public IEnumerator<ItemStack?> GetEnumerator() => _stacks.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _stacks.Count;

    public ItemStack? this[int index] => _stacks[index];

    public bool HasSlot(int slot) => slot >= 0 && slot < _stacks.Count;

    /// <summary>
    /// Add a stack to the inventory. If inventory space is insufficient, the remainder is returned.
    /// </summary>
    public ItemStack? Add(ItemStack stack)
    {
        long remainingCount = stack.Count;
        for (int i = 0; i < _stacks.Count && remainingCount > 0; i++)
        {
            ItemStack? existingStack = _stacks[i];
            if (existingStack?.Item == stack.Item && existingStack.Count < existingStack.Item.MaxStackCount)
            {
                long freeSpace = existingStack.Item.MaxStackCount - existingStack.Count;
                long transferredCount = Math.Min(remainingCount, freeSpace);
                remainingCount -= transferredCount;
                _stacks[i] = new ItemStack(existingStack.Item, existingStack.Count + transferredCount);
            }
        }

        if (remainingCount == 0) return null;

        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i] == null)
            {
                _stacks[i] = new ItemStack(stack.Item, remainingCount);
                return null;
            }
        }
        return new ItemStack(stack.Item, remainingCount);
    }

    /// <summary>
    /// Put an item stack into a target slot. If the slot contains a stack that can be fully or partially merged with
    /// the put stack, it's merged and a remainder is returned. Otherwise, a stack that was in the slot previously is
    /// returned.
    /// </summary>
    public ItemStack? PutAt(int slot, ItemStack stack)
    {
        ItemStack? existingStack = _stacks[slot];
        bool canMerge = existingStack != null &&
            stack.Item == existingStack.Item &&
            existingStack.FreeSpace > 0;

        if (canMerge)
        {
            long movedCount = Math.Min(stack.Count, existingStack.Required.FreeSpace);
            _stacks[slot] = new ItemStack(existingStack.Item, existingStack.Count + movedCount);
            long remainder = stack.Count - movedCount;
            return remainder == 0 ? null : new ItemStack(stack.Item, remainder);
        }

        _stacks[slot] = stack;
        return existingStack;
    }

    public ItemStack? RemoveAt(int slot)
    {
        ItemStack? result = _stacks[slot];
        _stacks[slot] = null;
        return result;
    }

    public IReadOnlyList<ItemStack?> ToSnapshot() => this.ToImmutableList();

    public void ReplicateSnapshot(IReadOnlyList<ItemStack?> stacks) => _stacks = stacks.ToList();

    public void ApplyDelta(ListDelta<ItemStack?> delta) => delta.MutateList(_stacks);
}
