using Soteo.Core.Items;

namespace Soteo.Core.Dto;

public sealed record ItemStack
{
    public Item Item { get; }
    public long Count { get; }

    public ItemStack(Item item, long count)
    {
        if (count <= 0) throw new ArgumentException("Size must be positive");

        Item = item;
        Count = count;
    }

    public static ItemStack Of<T>(long count) where T : Item, new() => new(Item.Instance<T>(), count);

    public ItemStack WithCount(long count) => new (Item, count);

    public long FreeSpace => Item.MaxStackCount - Count;
}
