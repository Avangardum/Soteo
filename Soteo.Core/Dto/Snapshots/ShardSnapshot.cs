namespace Soteo.Core.Shared.Dto.Snapshots;

public sealed record ShardSnapshot
{
    public required long Tick { get; init; }
    public required IReadOnlyDictionary<Guid, EntitySnapshot> Entities { get; init; }
}
