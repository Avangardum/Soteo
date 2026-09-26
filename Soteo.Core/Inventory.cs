using System.Collections;
using Soteo.Core.Dto;

namespace Soteo.Core;

public sealed class Inventory : IReadOnlyList<ItemStack?>
{
    private readonly ItemStack?[] _stacks;

    public Inventory(int size)
    {
        _stacks = new ItemStack?[size];
    }

    public Inventory(IReadOnlyList<ItemStack?> stacks)
    {
        _stacks = stacks.ToArray();
    }

    public IEnumerator<ItemStack?> GetEnumerator() => _stacks.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => _stacks.Length;

    public ItemStack? this[int index] => _stacks[index];

    public bool TryAdd(ItemStack stack)
    {
        // todo merge stacks
        for (int i = 0; i < _stacks.Length; i++)
        {
            if (_stacks[i] == null)
            {
                _stacks[i] = stack;
                return true;
            }
        }
        return false;
    }
}
