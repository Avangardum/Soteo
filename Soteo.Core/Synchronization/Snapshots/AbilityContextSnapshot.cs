namespace Soteo.Core.Synchronization.Snapshots;

public sealed record AbilityContextSnapshot
{
    public required Ability Ability { get; init; }
    public required Guid UserId { get; init; }
    public bool Alt { get; init; }
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Guid? TargetUnitId { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
}
