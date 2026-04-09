using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Extensions;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public sealed record EntitySnapshot
{
    public required Guid Id { get; init; }
    public Vector2? Position { get; init; }
    public float? Azimuth { get; init; }
    
    public static EntitySnapshot Interpolate(EntitySnapshot from, EntitySnapshot to, float weight)
    {
        if (from.Id != to.Id) throw new ArgumentException();
        return new()
        {
            Id = to.Id,
            Position = InterpolateNullable(from.Position, to.Position, (f, t) => f.Lerp(t, weight)),
            Azimuth = InterpolateNullable(from.Azimuth, to.Azimuth, (f, t) => ModularLerp(f, t, weight, 360))
        };
    }
    
    public void ApplyToEntity(IEntity entity)
    {
        if (Position != null) entity.Position = Position.Value;
        if (Azimuth != null) entity.Azimuth = Azimuth.Value;
    }
}