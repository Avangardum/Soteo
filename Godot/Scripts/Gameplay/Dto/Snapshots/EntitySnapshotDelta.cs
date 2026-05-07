namespace Soteo.Gameplay.Dto.Snapshots;

public abstract record EntitySnapshotDelta
{
    public required Guid Id { get; init; }
    public Delta<Vector2> Position { get; init; }
    public Delta<double> Azimuth { get; init; }
}