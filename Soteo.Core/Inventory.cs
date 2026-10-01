using System.Collections;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;
using Soteo.Core.Dto.Snapshots;

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
        // todo merge stacks
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i] == null)
            {
                _stacks[i] = stack;
                return true;
            }
        }
        return false;
    }

    public void ReplicateSnapshot(IReadOnlyList<ItemStack?> stacks) => _stacks = stacks.ToList();

    public void ApplyDelta(ListDelta<ItemStack?> delta) => delta.MutateList(_stacks);
}
