namespace Soteo.Core.Synchronization.Deltas;

public sealed record ShardDelta
{
    public required long Tick { get; init; }
    public required DictionaryDelta<Guid, EntityDelta> Entities { get; init; }

    public static ShardDelta Between(ShardSnapshot from, ShardSnapshot to)
    {
        return new ShardDelta
        {
            Tick = to.Tick,
            Entities = new DictionaryDelta<Guid, EntityDelta>
            {
                Changes = to.Entities.Values
                    .Select(it => it.DeltaFrom(from.Entities.GetValueOrDefault(it.Id)))
                    .Where(it => it.HasChanged)
                    .ToDictionary(it => it.Id, it => it).AsReadOnly(),
                RemovedKeys = from.Entities.Keys.Except(to.Entities.Keys).ToList().AsReadOnly()
            }
        };
    }
}
