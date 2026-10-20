using Soteo.Core.Dto;
using Soteo.Core.Dto.Deltas;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Entities;

public interface IUnitPuppet : IEntity
{
    bool IsDead { get; }
    IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses { get; }

    /// <summary>
    /// Inventory displayed in the HUD. Usually it's automatically synced with a shard server, just like the normal
    /// inventory. For lag compensation, it can be temporarily locked, shortly stopping server sync, then manual
    /// changes can be applied.
    /// </summary>
    Inventory VisualInventory { get; }

    Inventory Inventory { get; }
    IReadOnlyDictionary<Stat, double> Stats { get; }
    IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; }
    IReadOnlyDictionary<EquipmentSlot, EquipmentSlotState> EquipmentSlotStates { get; }
    AbilityUseProgress? AbilityUseProgress { get; }
    IReadOnlyList<Guid> ControllingPlayerIds { get; }
    Faction Faction { get; }

    EntitySnapshot ToSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
    bool IsAlliedTo(IUnitPuppet other);
    void Respawn(IUnitPuppetNode node);

    /// <summary>
    /// Temporarily lock the visual inventory, preventing it from receiving server updates
    /// </summary>
    void LockVisualInventory();
}
