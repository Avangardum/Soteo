using Soteo.Core.Items;

namespace Soteo.Core.Dto;

public sealed record ItemStack
{
    public Item Item { get; }
    public int Size { get; }

    public ItemStack(Item item, int size)
    {
        if (size <= 0) throw new ArgumentException("Size must be positive");

        Item = item;
        Size = size;
    }

    public static ItemStack Of<T>(int count) where T : Item, new() => new(Item.Instance<T>(), count);
}
