namespace Soteo.Core.Shared.Dto.Snapshots;

public sealed record PersistenceShardSnapshot
{
    public required IReadOnlyDictionary<Guid, EntitySnapshot> Entities { get; init; }
}
