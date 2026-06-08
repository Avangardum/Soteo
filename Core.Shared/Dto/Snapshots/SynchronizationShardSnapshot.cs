namespace Soteo.Core.Shared.Dto.Snapshots;

public sealed record SynchronizationShardSnapshot
{
    public required IReadOnlyDictionary<Guid, EntitySnapshot> Entities { get; init; }
}
