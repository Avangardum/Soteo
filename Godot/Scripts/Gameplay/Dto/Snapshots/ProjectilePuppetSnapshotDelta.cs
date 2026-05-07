namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record ProjectilePuppetSnapshotDelta : EntitySnapshotDelta
{
    public static ProjectilePuppetSnapshotDelta Between(ProjectilePuppetSnapshot from, ProjectilePuppetSnapshot to)
    {
        if (from.Id != to.Id) throw new ArgumentException();
        
        return new ProjectilePuppetSnapshotDelta
        {
            Id = to.Id,
            Position = Delta.Between(to.Position, from.Position),
            Azimuth = Delta.Between(to.Azimuth, from.Azimuth),
        };
    }
}