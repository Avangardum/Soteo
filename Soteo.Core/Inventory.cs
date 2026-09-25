using System.Collections;
using Soteo.Core.Dto;

namespace Soteo.Core;

public sealed class Inventory(int size) : IReadOnlyList<ItemStack?>
{
    private readonly ItemStack?[] _stacks = new ItemStack?[size];

    public IEnumerator<ItemStack?> GetEnumerator() => _stacks.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => size;

    public ItemStack? this[int index] => _stacks[index];

    public bool TryAdd(ItemStack stack)
    {
        // todo merge stacks
        for (int i = 0; i < size; i++)
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
