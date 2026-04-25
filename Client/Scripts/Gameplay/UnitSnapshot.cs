using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public sealed record UnitSnapshot(Guid Id) : EntitySnapshot<UnitSnapshot>(Id)
{
    public ImmutableDictionary<Stat, float> Stats { get; init; } = [];
    public ImmutableDictionary<AbilitySlot, IReadOnlyAbilityState> AbilityStates { get; init; } = [];
    public AbilitySlot? CurrentAbilitySlot { get; init; }
    public float? CurrentAbilityRemainingUseTime { get; init; }
    
    public override UnitSnapshot Interpolate(UnitSnapshot to, float weight)
    {
        UnitSnapshot from = this;
        return base.Interpolate(to, weight) with
        {
            AbilityStates = InterpolateAbilityStates(from.AbilityStates, to.AbilityStates, weight),
            CurrentAbilityRemainingUseTime = InterpolateNullable(from.CurrentAbilityRemainingUseTime,
                to.CurrentAbilityRemainingUseTime, (f, t) => t == -1 ? -1 : LerpDecrease(f, t, weight))
        };
    }
    
    private static ImmutableDictionary<AbilitySlot, IReadOnlyAbilityState> InterpolateAbilityStates
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
}