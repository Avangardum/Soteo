using System.Collections.Immutable;

namespace Soteo.Gameplay;

public sealed record ShardSnapshot
{
    public required ImmutableList<EntitySnapshot> Entities { get; init; }
}