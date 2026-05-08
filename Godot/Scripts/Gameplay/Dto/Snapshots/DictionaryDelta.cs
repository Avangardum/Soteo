using System.Collections.Immutable;

namespace Soteo.Gameplay.Dto.Snapshots;

public static class DictionaryDelta
{
    public static DictionaryDelta<TKey, TValue> Between<TKey, TValue>
    (
        IReadOnlyDictionary<TKey, TValue> from,
        IReadOnlyDictionary<TKey, TValue> to
    ) where TKey : notnull
    {
        ImmutableDictionary<TKey, TValue> changes = to
            .Where(it => !from.TryGetValue(it.Key, out TValue value) || !Equals(value, it.Value))
            .ToImmutableDictionary();
        ImmutableList<TKey> removedKeys = from.Keys.Except(to.Keys).ToImmutableList();
        return new DictionaryDelta<TKey, TValue> { Changes = changes, RemovedKeys = removedKeys };
    }
    
    public static DictionaryDelta<TKey, TValue> FromNewDictionary<TKey, TValue>
    (
        IReadOnlyDictionary<TKey, TValue> dictionary
    ) where TKey : notnull
    {
        return new DictionaryDelta<TKey, TValue> { Changes = dictionary };
    }
}

public sealed class DictionaryDelta<TKey, TValue> where TKey : notnull
{
    public IReadOnlyDictionary<TKey, TValue> Changes { get; init; } = ImmutableDictionary<TKey, TValue>.Empty;
    public IReadOnlyList<TKey> RemovedKeys { get; init; } = [];
    
    public bool HasChanged => Changes.Count > 0 || RemovedKeys.Count > 0;
    
    public void MutateDictionary
    (
        IDictionary<TKey, TValue> dictionary,
        double interpolationWeight,
        Func<TValue, TValue, double, TValue> interpolateValue
    )
    {
        foreach ((TKey key, TValue newValue) in Changes)
            dictionary[key] = dictionary.TryGetValue(key, out TValue? oldValue) ?
                interpolateValue(oldValue, newValue, interpolationWeight) : newValue;
    
        foreach (TKey key in RemovedKeys)
            dictionary.Remove(key);
    }
}
