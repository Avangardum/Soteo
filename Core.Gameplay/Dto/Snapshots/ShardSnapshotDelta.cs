using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Dto.Snapshots;

public sealed record ShardSnapshotDelta
{
    public required DictionaryDelta<Guid, EntitySnapshotDelta> Entities { get; init; }
    
    public static ShardSnapshotDelta Between(ShardSnapshot from, ShardSnapshot to)
    {
        return new ShardSnapshotDelta
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
