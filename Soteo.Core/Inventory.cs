using System.Collections;
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

    public bool TryAdd(ItemStack stack)
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

        if (remainingCount == 0) return true;

        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i] == null)
            {
                _stacks[i] = new ItemStack(stack.Item, remainingCount);
                return true;
            }
        }
        return false;
    }

    public void ReplicateSnapshot(IReadOnlyList<ItemStack?> stacks) => _stacks = stacks.ToList();

    public void ApplyDelta(ListDelta<ItemStack?> delta) => delta.MutateList(_stacks);
}
