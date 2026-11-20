namespace Soteo.Core.Snapshots;

public sealed record PlayerCharacterTrackerSnapshot
{
    public required Guid Id { get; init; }
    public required Guid PlayerId { get; init; }
    public required Guid? ShardId { get; init; }
}
