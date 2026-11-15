using System.Collections;

namespace Soteo.Core.Entities.Units;

public sealed class SimpleInventory : Inventory
{
    public override event Action Changed = delegate { };

    private List<ItemStack?> _stacks;

    public SimpleInventory(int size)
    {
        _stacks = Enumerable.Repeat<ItemStack?>(null, size).ToList();
    }

    public SimpleInventory(IReadOnlyList<ItemStack?> stacks)
    {
        _stacks = stacks.ToList();
    }

    public override IEnumerator<ItemStack?> GetEnumerator() => _stacks.GetEnumerator();

    public override int Count => _stacks.Count;

    public override ItemStack? this[int index] => _stacks[index];

    public override bool HasSlot(int slot) => slot >= 0 && slot < _stacks.Count;

    public override ItemStack? Add(ItemStack stack)
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

    public override ItemStack? Put(int slot, ItemStack stack)
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

    public override ItemStack? Remove(int slot)
    {
        ItemStack? result = _stacks[slot];
        _stacks[slot] = null;
        if (result != null) Changed();
        return result;
    }

    public override InventorySnapshot ToSnapshot() => new(_stacks.ToList().AsReadOnly());

    public override void ReplicateSnapshot(InventorySnapshot snapshot)
    {
        _stacks = snapshot.Stacks.ToList();
        Changed();
    }

    public override void ApplyDelta(InventoryDelta delta)
    {
        if (!delta.HasChanged) return;
        delta.Stacks.MutateList(_stacks);
        Changed();
    }
}
