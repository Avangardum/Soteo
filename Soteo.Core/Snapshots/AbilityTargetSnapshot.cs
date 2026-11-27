namespace Soteo.Core.Snapshots;

public sealed record AbilityTargetSnapshot
{
    public static readonly AbilityTargetSnapshot Nothing = new();

    public Vector2? Position { get; init; }
    public Guid? UnitId { get; init; }
    public Vector2? Direction { get; init; }
    public Guid? ShardId { get; init; }
}
