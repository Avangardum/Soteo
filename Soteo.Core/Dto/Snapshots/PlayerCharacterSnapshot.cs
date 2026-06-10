namespace Soteo.Core.Dto.Snapshots;

public sealed record PlayerCharacterSnapshot
{
    public required Guid Id { get; init; }
    public required Guid? PlayerId { get; init; }
    public required Guid? ShardId { get; init; }
}
