using System.Collections;

namespace Soteo.Core.Entities.Units;

public sealed class Inventory : IInventory
{
    public event Action Changed = delegate { };

    private List<ItemStack?> _stacks;

    public Inventory(int size)
    {
        _stacks = Enumerable.Repeat<ItemStack?>(null, size).ToList();
    }

    public Inventory(IReadOnlyList<ItemStack?> stacks)
    {
        _stacks = stacks.ToList();
    }

    public IEnumerator<ItemStack?> GetEnumerator() => _stacks.GetEnumerator();

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
            if (existingStack?.Item == stack.Item && existingStack.FreeSpace > 0)
            {
                long transferredCount = Math.Min(remainingCount, existingStack.FreeSpace);
                remainingCount -= transferredCount;
                _stacks[i] = existingStack.WithCount(existingStack.Count + transferredCount);
            }
        }

        if (remainingCount == 0)
        {
            Changed();
            return null;
        }

        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i] == null)
            {
                _stacks[i] = new ItemStack(stack.Item, remainingCount);
                Changed();
                return null;
            }
        }

        if (remainingCount < stack.Count)
        {
            Changed();
            return new ItemStack(stack.Item, remainingCount);
        }

        return stack;
    }

    /// <summary>
    /// Put an item stack into a target slot. If the slot contains a stack that can be fully or partially merged with
    /// the put stack, it's merged and a remainder is returned. Otherwise, a stack that was in the slot previously is
    /// returned.
    /// </summary>
    public ItemStack? Put(int slot, ItemStack stack)
    {
        ItemStack? existingStack = _stacks[slot];
        bool canMerge = existingStack != null &&
            stack.Item == existingStack.Item &&
            existingStack.FreeSpace > 0;

        if (canMerge)
        {
            long movedCount = Math.Min(stack.Count, existingStack.Required.FreeSpace);
            _stacks[slot] = existingStack.WithCount(existingStack.Count + movedCount);
            long remainder = stack.Count - movedCount;
            Changed();
            return remainder == 0 ? null : new ItemStack(stack.Item, remainder);
        }

        _stacks[slot] = stack;
        Changed();
        return existingStack;
    }

    public ItemStack? Remove(int slot)
    {
        ItemStack? result = _stacks[slot];
        _stacks[slot] = null;
        if (result != null) Changed();
        return result;
    }

    public IReadOnlyList<ItemStack?> ToSnapshot() => this.ToList().AsReadOnly();

    public void ReplicateSnapshot(IReadOnlyList<ItemStack?> stacks)
    {
        _stacks = stacks.ToList();
        Changed();
    }

    public void ApplyDelta(ListDelta<ItemStack?> delta)
    {
        if (!delta.HasChanged) return;
        delta.MutateList(_stacks);
        Changed();
    }
}
