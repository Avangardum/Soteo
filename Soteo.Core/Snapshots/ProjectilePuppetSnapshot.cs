using Soteo.Core.Deltas;

namespace Soteo.Core.Snapshots;

public sealed record ProjectilePuppetSnapshot : EntitySnapshot<ProjectilePuppetSnapshot>
{
    public override EntityDelta DeltaFrom(ProjectilePuppetSnapshot? from)
    {
        if (from == null)
        {
            return new ProjectilePuppetDelta
            {
                Id = Id,
                Position = Position,
                Azimuth = Azimuth
            };
        }

        if (from.Id != Id) throw new ArgumentException();

        return new ProjectilePuppetDelta
        {
            Id = Id,
            Position = Delta.Between(from.Position, Position),
            Azimuth = Delta.Between(from.Azimuth, Azimuth)
        };
    }

    public override EntitySnapshot ToPuppet() => this;
}
