using Soteo.Shared.Extensions;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public abstract record EntitySnapshot
{
    public required Guid Id { get; init; }
    public required Vector2 Position { get; init; }
    public required float Azimuth { get; init; }
    
    public abstract EntitySnapshot Interpolate(EntitySnapshot to, float weight);
}

public abstract record EntitySnapshot<T> : EntitySnapshot where T : EntitySnapshot<T>
{
    public virtual T Interpolate(T to, float weight)
    {
        EntitySnapshot<T> from = this;
        if (from.Id != to.Id) throw new ArgumentException();
        return to with
        {
            Position = from.Position.Lerp(to.Position, weight),
            Azimuth = ModularLerp(from.Azimuth, to.Azimuth, weight, 360)
        };
    }

    public sealed override EntitySnapshot Interpolate(EntitySnapshot to, float weight) => Interpolate((T)to, weight);
}