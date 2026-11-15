using System.Collections;

namespace Soteo.Core.Entities.Units;

public abstract class Inventory : IReadOnlyList<ItemStack?>
{
    public abstract event Action Changed;
    public abstract int Count { get; }
    public abstract bool HasSlot(int slot);

    /// <summary>
    /// Add a stack to the inventory, returning a remainder of the stack if it didn't fit
    /// </summary>
    public abstract ItemStack? Add(ItemStack stack);

    /// <inheritdoc cref="Add"/>
    public ItemStack? Add<T>(long count) where T : Item, new() =>
        Add(ItemStack.Of<T>(count));

    /// <summary>
    /// Put an item stack into a target slot, returning a remainder of the stack if it didn't fit
    /// or a stack that it replaced
    /// </summary>
    public abstract ItemStack? Put(int slot, ItemStack stack);

    public ItemStack? Put<T>(int slot, long count) where T : Item, new() =>
        Put(slot, ItemStack.Of<T>(count));

    public abstract ItemStack? Remove(int slot);
    public abstract InventorySnapshot ToSnapshot();
    public abstract void ReplicateSnapshot(InventorySnapshot snapshot);
    public abstract void ApplyDelta(InventoryDelta delta);
    public abstract IEnumerator<ItemStack?> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public abstract ItemStack? this[int index] { get; }
}
