using Soteo.Shared.Extensions;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public abstract record EntitySnapshot(Guid Id)
{
    public Vector2? Position { get; init; }
    public float? Azimuth { get; init; }
    
    public abstract EntitySnapshot Interpolate(EntitySnapshot to, float weight);
}

public abstract record EntitySnapshot<T>(Guid Id) : EntitySnapshot(Id) where T : EntitySnapshot<T>
{
    public virtual T Interpolate(T to, float weight)
    {
        EntitySnapshot<T> from = this;
        if (from.Id != to.Id) throw new ArgumentException();
        return to with
        {
            Position = InterpolateNullable(from.Position, to.Position, (f, t) => f.Lerp(t, weight)),
            Azimuth = InterpolateNullable(from.Azimuth, to.Azimuth, (f, t) => ModularLerp(f, t, weight, 360))
        };
    }

    public sealed override EntitySnapshot Interpolate(EntitySnapshot to, float weight) => Interpolate((T)to, weight);
}