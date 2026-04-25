using System.Collections.Immutable;
using Soteo.Shared.Extensions;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public sealed record ShardSnapshot
{
    public required ImmutableList<EntitySnapshot> Entities { get; init; }
    
    public ShardSnapshot Interpolate(ShardSnapshot to, float weight)
    {
        ShardSnapshot from = this;
        ImmutableDictionary<Guid, EntitySnapshot> fromEntities = from.Entities.ToImmutableDictionary(it => it.Id);
        
        return new ShardSnapshot
        {
            Entities = to.Entities
                .Select(t => fromEntities.TryGetValue(t.Id, out EntitySnapshot? f) ? f.Interpolate(t, weight) : t)
                .ToImmutableList()
        };
    }
}