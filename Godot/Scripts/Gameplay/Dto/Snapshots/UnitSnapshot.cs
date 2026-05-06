using Soteo.Shared.Enums;
using static Soteo.Shared.Maths;

namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record UnitSnapshot : EntitySnapshot<UnitSnapshot>
{
    public required bool IsMoving { get; init; }
    public required IReadOnlyDictionary<Stat, double> Stats { get; init; }
    public required IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilityStates { get; init; }
    public required AbilityUseProgress? AbilityUseProgress { get; init; }
    public required IReadOnlyDictionary<Guid, DeflatedStatusContext> Statuses { get; init; }
    
    public override UnitSnapshot Interpolate(UnitSnapshot to, double weight)
    {
        UnitSnapshot from = this;
        return base.Interpolate(to, weight) with
        {
            AbilityStates = InterpolateDictionary(from.AbilityStates, to.AbilityStates, weight, InterpolateAbilityState),
            AbilityUseProgress = InterpolateNullable(from.AbilityUseProgress, to.AbilityUseProgress, weight,
                InterpolateAbilityUseProgress),
            Statuses = InterpolateDictionary(from.Statuses, to.Statuses, weight, InterpolateStatusContext)
        };
    }
    
    private AbilitySlotState InterpolateAbilityState(AbilitySlotState from, AbilitySlotState to, double weight) =>
        to with { Cooldown = LerpDecrease(from.Cooldown, to.Cooldown, weight) };
    
    private DeflatedStatusContext InterpolateStatusContext
    (
        DeflatedStatusContext from,
        DeflatedStatusContext to,
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
}