using Soteo.Core.Gameplay.Enums;
using static Soteo.Util.Maths;

namespace Soteo.Core.Gameplay.Dto.Snapshots;

public record UnitPuppetSnapshot : EntitySnapshot<UnitPuppetSnapshot>
{
    public required bool IsDead { get; init; }
    public required bool IsMoving { get; init; }
    public required IReadOnlyDictionary<Stat, double> Stats { get; init; }
    public required IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; }
    public required AbilityUseProgress? AbilityUseProgress { get; init; }
    public required IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses { get; init; }
    
    public override EntitySnapshotDelta DeltaFrom(UnitPuppetSnapshot? from)
    {
        if (from == null)
        {
            return new UnitPuppetSnapshotDelta
            {
                Id = Id,
                Position = Position,
                Azimuth = Azimuth,
                IsDead = IsDead,
                IsMoving = IsMoving,
                Stats = DictionaryDelta.FromNewDictionary(Stats),
                AbilitySlotStates = DictionaryDelta.FromNewDictionary(AbilitySlotStates),
                AbilityUseProgress = AbilityUseProgress,
                Statuses = DictionaryDelta.FromNewDictionary(Statuses)
            };
        }
        
        if (from.Id != Id) throw new ArgumentException();
        
        return new UnitPuppetSnapshotDelta
        {
            Id = Id,
            Position = Delta.Between(from.Position, Position),
            Azimuth = Delta.Between(from.Azimuth, Azimuth),
            IsDead = Delta.Between(from.IsDead, IsDead),
            IsMoving = Delta.Between(from.IsMoving, IsMoving),
            Stats = DictionaryDelta.Between(from.Stats, Stats),
            AbilitySlotStates = DictionaryDelta.Between(from.AbilitySlotStates, AbilitySlotStates),
            AbilityUseProgress = Delta.Between(from.AbilityUseProgress, AbilityUseProgress),
            Statuses = DictionaryDelta.Between(from.Statuses, Statuses)
        };
    }
}
