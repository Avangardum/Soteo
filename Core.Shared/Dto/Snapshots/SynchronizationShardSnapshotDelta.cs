using System.Collections.Immutable;

namespace Soteo.Core.Shared.Dto.Snapshots;

public sealed record SynchronizationShardSnapshotDelta
{
    public required DictionaryDelta<Guid, EntitySnapshotDelta> Entities { get; init; }
    
    public static SynchronizationShardSnapshotDelta Between(SynchronizationShardSnapshot from, SynchronizationShardSnapshot to)
    {
        return new SynchronizationShardSnapshotDelta
        {
            Entities = new DictionaryDelta<Guid, EntitySnapshotDelta>
            {
                Changes = to.Entities.Values
                    .Select(it => it.DeltaFrom(from.Entities.GetOrDefault(it.Id)))
                    .Where(it => it.HasChanged)
                    .ToImmutableDictionary(it => it.Id, it => it),
                RemovedKeys = from.Entities.Keys.Except(to.Entities.Keys).ToImmutableList()
            }
        };
    }
}
