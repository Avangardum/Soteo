using System.Collections;

namespace Soteo.Core.Entities.Units;

public abstract class EquipmentInventory : IReadOnlyDictionary<EquipmentSlot, ItemStack?>
{
    public abstract event Action Changed;

    /// <summary>
    /// Put an item stack into a target slot, returning a remainder of the stack if it didn't fit,
    /// or a stack it replaced
    /// </summary>
    public abstract ItemStack? Put(EquipmentSlot slot, ItemStack stack);

    public ItemStack? Put<T>(EquipmentSlot slot, long count) where T : Item, new() =>
        Put(slot, ItemStack.Of<T>(count));

    public abstract ItemStack? Remove(EquipmentSlot slot);

    /// <summary>
    /// Slots that are empty, but blocked by items in other slots as keys and slots that block them as values
    /// </summary> // todo to method
    public abstract IReadOnlyDictionary<EquipmentSlot, EquipmentSlot> BlockedAndBlockingSlots { get; }

    public abstract int Count { get; }

    public abstract IEnumerable<EquipmentSlot> Keys { get; }

    public abstract IEnumerable<ItemStack?> Values { get; }

    public abstract EquipmentInventorySnapshot ToSnapshot();

    public abstract void ReplicateSnapshot(EquipmentInventorySnapshot snapshot);

    public abstract void ApplyDelta(EquipmentInventoryDelta delta);

    public abstract IEnumerator<KeyValuePair<EquipmentSlot, ItemStack?>> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public abstract bool ContainsKey(EquipmentSlot key);

    public abstract bool TryGetValue(EquipmentSlot key, out ItemStack? value);

    public abstract ItemStack? this[EquipmentSlot key] { get; }
}
