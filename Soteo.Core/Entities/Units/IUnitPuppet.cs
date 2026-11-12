namespace Soteo.Core.Entities.Units;

public interface IUnitPuppet : IEntity
{
    bool IsDead { get; }
    IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses { get; }
    IInventory Inventory { get; }
    IEquipmentInventory EquipmentInventory { get; }
    IReadOnlyDictionary<Stat, double> Stats { get; }
    IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; }
    AbilityUseProgress? AbilityUseProgress { get; }
    IReadOnlyList<Guid> ControllingPlayerIds { get; }
    Faction Faction { get; }

    EntitySnapshot ToSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
    bool IsAlliedTo(IUnitPuppet other);
    void Respawn(IUnitPuppetNode node);

    /// <summary>
    /// Temporarily desynchronize the inventory, preventing it from receiving server updates
    /// </summary>
    void DesyncInventory();
}
