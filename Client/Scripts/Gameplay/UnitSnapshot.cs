using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public sealed record UnitSnapshot : EntitySnapshot<UnitSnapshot>
{
    public required bool IsMoving { get; init; }
    public required ImmutableDictionary<Stat, float> Stats { get; init; }
    public required ImmutableDictionary<AbilitySlot, IReadOnlyAbilityState> AbilityStates { get; init; }
    public required AbilityUseProgress? AbilityUseProgress { get; init; }
    
    public override UnitSnapshot Interpolate(UnitSnapshot to, float weight)
    {
        UnitSnapshot from = this;
        return base.Interpolate(to, weight) with
        {
            AbilityStates = InterpolateAbilityStates(from.AbilityStates, to.AbilityStates, weight),
            AbilityUseProgress = InterpolateNullable(from.AbilityUseProgress, to.AbilityUseProgress, weight,
                InterpolateAbilityUseProgress)
        };
    }
    
    private ImmutableDictionary<AbilitySlot, IReadOnlyAbilityState> InterpolateAbilityStates
    (
        ImmutableDictionary<AbilitySlot, IReadOnlyAbilityState> from,
        ImmutableDictionary<AbilitySlot, IReadOnlyAbilityState> to,
        float weight
    )
    {
        return to.ToImmutableDictionary(pair => pair.Key, pair =>
        {
            IReadOnlyAbilityState t = pair.Value;
            if (!from.TryGetValue(pair.Key, out IReadOnlyAbilityState? f)) return t;
            return new AbilityState(t) { Cooldown = LerpDecrease(f.Cooldown, t.Cooldown, weight) };
        });
    }
    
    private static AbilityUseProgress InterpolateAbilityUseProgress
    (
        AbilityUseProgress from,
        AbilityUseProgress to,
        float weight
    )
    {
        return to with
        {
            ElapsedTime = LerpIncrease(from.ElapsedTime, to.ElapsedTime, weight),
            RemainingTime = LerpDecrease(from.RemainingTime, to.RemainingTime, weight)
        };
    }
}