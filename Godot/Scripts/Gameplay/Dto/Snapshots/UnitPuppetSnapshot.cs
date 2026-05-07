using Soteo.Shared.Enums;
using static Soteo.Shared.Maths;

namespace Soteo.Gameplay.Dto.Snapshots;

public record UnitPuppetSnapshot : PuppetEntitySnapshot<UnitPuppetSnapshot>
{
    public required bool IsMoving { get; init; }
    public required IReadOnlyDictionary<Stat, double> Stats { get; init; }
    public required IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; init; }
    public required AbilityUseProgress? AbilityUseProgress { get; init; }
    public required IReadOnlyDictionary<Guid, PuppetStatusContext> Statuses { get; init; }
    
    public override UnitPuppetSnapshot Interpolate(UnitPuppetSnapshot to, double weight)
    {
        UnitPuppetSnapshot from = this;
        return base.Interpolate(to, weight) with
        {
            AbilitySlotStates = InterpolateDictionary(from.AbilitySlotStates, to.AbilitySlotStates, weight, InterpolateAbilityState),
            AbilityUseProgress = InterpolateNullable(from.AbilityUseProgress, to.AbilityUseProgress, weight,
                InterpolateAbilityUseProgress),
            Statuses = InterpolateDictionary(from.Statuses, to.Statuses, weight, InterpolateStatusContext)
        };
    }
    
    private AbilitySlotState InterpolateAbilityState(AbilitySlotState from, AbilitySlotState to, double weight) =>
        to with { Cooldown = LerpDecrease(from.Cooldown, to.Cooldown, weight) };
    
    private PuppetStatusContext InterpolateStatusContext
    (
        PuppetStatusContext from,
        PuppetStatusContext to,
        double weight
    )
    {
        return to with { DisplayElapsedTime = LerpIncrease(from.DisplayElapsedTime, to.DisplayElapsedTime, weight) };
    }

    private AbilityUseProgress InterpolateAbilityUseProgress
    (
        AbilityUseProgress from,
        AbilityUseProgress to,
        double weight
    )
    {
        return to with
        {
            ElapsedTime = LerpIncrease(from.ElapsedTime, to.ElapsedTime, weight),
            RemainingTime = LerpDecrease(from.RemainingTime, to.RemainingTime, weight)
        };
    }

    public override EntitySnapshotDelta DeltaFrom(UnitPuppetSnapshot? from)
    {
        if (from == null)
        {
            return new UnitPuppetSnapshotDelta
            {
                Id = Id,
                Position = Position,
                Azimuth = Azimuth,
                IsMoving = IsMoving,
                Stats = DictionaryDelta.FromNewDictionary(Stats),
                AbilitySlotStates = DictionaryDelta.FromNewDictionary(AbilitySlotStates),
                AbilityUseProgress = AbilityUseProgress,
                Statuses = DictionaryDelta.FromNewDictionary(Statuses)
            };
        }
        
        if (from.Id != Id) throw new InvalidOperationException();
        
        return new UnitPuppetSnapshotDelta
        {
            Id = Id,
            Position = Delta.Between(from.Position, Position),
            Azimuth = Delta.Between(from.Azimuth, Azimuth),
            IsMoving = Delta.Between(from.IsMoving, IsMoving),
            Stats = DictionaryDelta.Between(from.Stats, Stats),
            AbilitySlotStates = DictionaryDelta.Between(from.AbilitySlotStates, AbilitySlotStates),
            AbilityUseProgress = Delta.Between(from.AbilityUseProgress, AbilityUseProgress),
            Statuses = DictionaryDelta.Between(from.Statuses, Statuses)
        };
    }
}
