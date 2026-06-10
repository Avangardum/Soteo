using System.Collections;

namespace Soteo.Util;

internal sealed class CovariantDictionaryWrapper<TKey, TValue, TInnerValue>
(
    IReadOnlyDictionary<TKey, TInnerValue> inner
) : IReadOnlyDictionary<TKey, TValue> where TInnerValue : TValue
{
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach ((TKey key, TInnerValue value) in inner)
            yield return new KeyValuePair<TKey, TValue>(key, value);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => inner.Count;

    public bool ContainsKey(TKey key) => inner.ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value)
    {
        bool found = inner.TryGetValue(key, out TInnerValue innerValue);
        value = innerValue;
        return found;
    }

    public TValue this[TKey key] => inner[key];

    public IEnumerable<TKey> Keys => inner.Keys;

    public IEnumerable<TValue> Values => inner.Values.Cast<TValue>();
}
