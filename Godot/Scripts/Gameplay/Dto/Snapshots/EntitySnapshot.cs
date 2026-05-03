using Soteo.Shared.Extensions;
using static Soteo.Shared.Maths;

namespace Soteo.Gameplay.Dto.Snapshots;

public abstract record EntitySnapshot
{
    public required Guid Id { get; init; }
    public required Vector2 Position { get; init; }
    public required double Azimuth { get; init; }
    
    public abstract EntitySnapshot Interpolate(EntitySnapshot to, double weight);
}

public abstract record EntitySnapshot<T> : EntitySnapshot where T : EntitySnapshot<T>
{
    public virtual T Interpolate(T to, double weight)
    {
        EntitySnapshot<T> from = this;
        if (from.Id != to.Id) throw new ArgumentException();
        return to with
        {
            Position = from.Position.Lerp(to.Position, weight),
            Azimuth = ModularLerp(from.Azimuth, to.Azimuth, weight, 360)
        };
    }

    public sealed override EntitySnapshot Interpolate(EntitySnapshot to, double weight) => Interpolate((T)to, weight);
}