using System.Collections;
using Soteo.Util.Interfaces;

namespace Soteo.Util;

public class CovariantReadOnlyDictionaryWrapper<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> dictionary) :
    ICovariantReadOnlyDictionary<TKey, TValue>
{
    public IEnumerator<IReadOnlyKeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            yield return pair.AsReadOnly();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

    public TValue this[TKey key] => dictionary[key];

    public IEnumerable<TKey> Keys => dictionary.Keys;

    public IEnumerable<TValue> Value => dictionary.Values;
}
