using System.Collections.Immutable;

namespace Soteo.Core.Gameplay.Dto.Snapshots;

public sealed record ShardSnapshot
{
    public required IReadOnlyDictionary<Guid, EntitySnapshot> Entities { get; init; }
    
    public ShardSnapshot Interpolate(ShardSnapshot to, double weight)
    {
        ShardSnapshot from = this;
        
        return new ShardSnapshot
        {
            Entities = to.Entities.ToImmutableDictionary
            (
                t => t.Key,
                t => from.Entities.TryGetValue(t.Key, out EntitySnapshot? f) ? f.Interpolate(t.Value, weight) : t.Value
            )
        };
    }
}