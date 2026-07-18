using System.Numerics;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Entities;

public interface IUnitPuppet : IEntity
{
    bool IsDead { get; }
    IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses { get; }
    IReadOnlyDictionary<Stat, double> Stats { get; }
    IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; }
    AbilityUseProgress? AbilityUseProgress { get; }
    Faction Faction { get; }
    EntitySnapshot ToSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
    bool IsAlliedTo(IUnitPuppet other);
    void Respawn(IUnitPuppetNode node);
}
