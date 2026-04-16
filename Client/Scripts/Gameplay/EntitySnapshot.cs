using System.Collections.Immutable;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public sealed record EntitySnapshot
{
    public required Guid Id { get; init; }
    public Vector2? Position { get; init; }
    public float? Azimuth { get; init; }
    public ImmutableDictionary<Stat, float> Stats { get; init; } = [];
    
    public static EntitySnapshot Interpolate(EntitySnapshot from, EntitySnapshot to, float weight)
    {
        if (from.Id != to.Id) throw new ArgumentException();
        return to with
        {
            Position = InterpolateNullable(from.Position, to.Position, (f, t) => f.Lerp(t, weight)),
            Azimuth = InterpolateNullable(from.Azimuth, to.Azimuth, (f, t) => ModularLerp(f, t, weight, 360))
        };
    }
}