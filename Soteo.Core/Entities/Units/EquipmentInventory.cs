using System.Collections;

namespace Soteo.Core.Entities.Units;

public class EquipmentInventory : IReadOnlyDictionary<EquipmentSlot, ItemStack?>
{
    public event Action Changed = delegate { };

    private Dictionary<EquipmentSlot, ItemStack?> _stacks;

    public EquipmentInventory(params IReadOnlyList<EquipmentSlot> slots)
    {
        _stacks = slots.ToDictionary(it => it, ItemStack? (it) => null);
    }

    public EquipmentInventory(IReadOnlyDictionary<EquipmentSlot, ItemStack?> stacks)
    {
        _stacks = stacks.ToDictionary();
    }

    /// <summary>
    /// Put an item stack into a target slot, returning a remainder of the stack if it didn't fit,
    /// or a stack it replaced
    /// </summary>
    public ItemStack? Put(EquipmentSlot slot, ItemStack stack)
    {
        if (stack.Item.EquippableTo != slot) return stack;
        IReadOnlyDictionary<EquipmentSlot, EquipmentSlot> blockedAndBlockingSlots = GetBlockedAndBlockingSlots();
        if (blockedAndBlockingSlots.ContainsKey(slot)) return stack;

        foreach (EquipmentSlot blockingSlot in stack.Item.BlocksEquipmentSlots)
        {
            if (_stacks[blockingSlot] != null) return stack;
            if (blockedAndBlockingSlots.ContainsKey(blockingSlot)) return stack;
        }

        ItemStack? existingStack = _stacks[slot];

        if (existingStack == null)
        {
            _stacks[slot] = stack;
            Changed();
            return null;
        }

        if (existingStack.Item != stack.Item || existingStack.FreeSpace == 0)
            return stack;

        long movedCount = Math.Min(existingStack.FreeSpace, stack.Count);
        _stacks[slot] = existingStack.WithCount(existingStack.Count + movedCount);
        Changed();
        return stack.WithCount(stack.Count - movedCount);
    }

    public ItemStack? Put<T>(EquipmentSlot slot, long count) where T : Item, new() =>
        Put(slot, ItemStack.Of<T>(count));

    public ItemStack? Remove(EquipmentSlot slot)
    {
        ItemStack? result = _stacks[slot];
        _stacks[slot] = null;
        if (result != null) Changed();
        return result;
    }

    /// <summary>
    /// Slots that are empty, but blocked by items in other slots as keys and slots that block them as values
    /// </summary>
    public IReadOnlyDictionary<EquipmentSlot, EquipmentSlot> GetBlockedAndBlockingSlots()
    {
        Dictionary<EquipmentSlot, EquipmentSlot> result = [];
        foreach ((EquipmentSlot blockingSlot, ItemStack? stack) in _stacks)
        {
            if (stack == null) continue;
            foreach (EquipmentSlot blockedSlot in stack.Item.BlocksEquipmentSlots)
                result[blockedSlot] = blockingSlot;
        }
        return result;
    }

    public int Count => _stacks.Count;

    public IEnumerable<EquipmentSlot> Keys => _stacks.Keys;

    public IEnumerable<ItemStack?> Values => _stacks.Values;

    public EquipmentInventorySnapshot ToSnapshot() => new(_stacks.ToDictionary());

    public void ReplicateSnapshot(EquipmentInventorySnapshot snapshot)
    {
        _stacks = snapshot.Stacks.ToDictionary();
        Changed();
    }

    public void ApplyDelta(EquipmentInventoryDelta delta)
    {
        if (!delta.HasChanged) return;
        delta.Stacks.MutateDictionary(_stacks);
        Changed();
    }

    public IEnumerator<KeyValuePair<EquipmentSlot, ItemStack?>> GetEnumerator() =>
        _stacks.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(EquipmentSlot key) => _stacks.ContainsKey(key);

    public bool TryGetValue(EquipmentSlot key, out ItemStack? value) => _stacks.TryGetValue(key, out value);

    public ItemStack? this[EquipmentSlot slot] => _stacks[slot];
}
