namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record ShardSnapshotDelta
{
    public required IReadOnlyList<EntitySnapshotDelta> Entities { get; init; }
}