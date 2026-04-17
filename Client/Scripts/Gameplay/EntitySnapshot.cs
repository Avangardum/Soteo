using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;
using static Soteo.Shared.SoteoMath;
using static Godot.Mathf;

namespace Soteo.Gameplay;

public sealed record EntitySnapshot
{
    public required Guid Id { get; init; }
    public Vector2? Position { get; init; }
    public float? Azimuth { get; init; }
    public ImmutableDictionary<Stat, float> Stats { get; init; } = [];
    public ImmutableDictionary<AbilitySlot, IReadOnlyAbilityState> AbilityStates { get; init; } = [];
    public AbilitySlot? CurrentAbilitySlot { get; init; }
    public float? CurrentAbilityRemainingUseTime { get; init; }
    
    public static EntitySnapshot Interpolate(EntitySnapshot from, EntitySnapshot to, float weight)
    {
        if (from.Id != to.Id) throw new ArgumentException();
        return to with
        {
            Position = InterpolateNullable(from.Position, to.Position, (f, t) => f.Lerp(t, weight)),
            Azimuth = InterpolateNullable(from.Azimuth, to.Azimuth, (f, t) => ModularLerp(f, t, weight, 360)),
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