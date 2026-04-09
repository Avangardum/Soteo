using System.Collections.Immutable;
using Soteo.Shared.Extensions;
using static Soteo.Shared.SoteoMath;

namespace Soteo.Gameplay;

public sealed record ShardSnapshot
{
    public required ImmutableList<EntitySnapshot> Entities { get; init; }
    
    public static ShardSnapshot Interpolate(ShardSnapshot from, ShardSnapshot to, float weight)
    {
        ImmutableDictionary<Guid, EntitySnapshot> fromEntities = from.Entities.ToImmutableDictionary(it => it.Id);
        
        return new()
        {
            Entities = to.Entities
                .Select(t => fromEntities.TryGetValue(t.Id, out EntitySnapshot? f) ?
                    EntitySnapshot.Interpolate(f, t, weight) : t)
                .ToImmutableList()
        };
    }
}