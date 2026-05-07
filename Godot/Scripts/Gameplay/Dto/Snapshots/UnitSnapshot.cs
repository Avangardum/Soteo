using System.Collections.Immutable;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record UnitSnapshot : EntitySnapshot<UnitSnapshot>
{
    public required bool IsMoving { get; init; }
    public required IReadOnlyDictionary<Stat, double> Stats { get; init; }
    public required IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; }
    public required AbilityUseProgress? AbilityUseProgress { get; init; }
    public required IReadOnlyDictionary<Guid, DeflatedStatusContext> Statuses { get; init; }

    public override EntitySnapshot ToPuppet()
    {
        return new UnitPuppetSnapshot
        {
            Id = Id,
            Position = Position,
            Azimuth = Azimuth,
            IsMoving = IsMoving,
            Stats = Stats,
            AbilitySlotStates = AbilitySlotStates,
            AbilityUseProgress = AbilityUseProgress,
            Statuses = Statuses.ToImmutableDictionary(it => it.Key, it => it.Value.ToPuppet()),
        };
    }

    public override EntitySnapshotDelta DeltaFrom(UnitSnapshot? from) =>
        throw new NotSupportedException();
}