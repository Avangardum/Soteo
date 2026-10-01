using System.Collections.Immutable;
using Soteo.Core.Dto.Snapshots;

namespace Soteo.Core.Dto.Deltas;

public sealed record ShardSnapshotDelta
{
    public required long Tick { get; init; }
    public required DictionaryDelta<Guid, EntitySnapshotDelta> Entities { get; init; }

    public static ShardSnapshotDelta Between(ShardSnapshot from, ShardSnapshot to)
    {
        return new ShardSnapshotDelta
        {
            Tick = to.Tick,
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
