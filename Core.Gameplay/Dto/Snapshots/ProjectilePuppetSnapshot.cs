namespace Soteo.Core.Gameplay.Dto.Snapshots;

public sealed record ProjectilePuppetSnapshot : EntitySnapshot<ProjectilePuppetSnapshot>
{
    public override EntitySnapshotDelta DeltaFrom(ProjectilePuppetSnapshot? from)
    {
        if (from == null)
        {
            return new ProjectilePuppetSnapshotDelta
            {
                Id = Id,
                Position = Position,
                Azimuth = Azimuth,
            };
        }
        
        if (from.Id != Id) throw new ArgumentException();
        
        return new ProjectilePuppetSnapshotDelta
        {
            Id = Id,
            Position = Delta.Between(from.Position, Position),
            Azimuth = Delta.Between(from.Azimuth, Azimuth),
        };
    }

    public override EntitySnapshot ToPuppet() => this;
}
