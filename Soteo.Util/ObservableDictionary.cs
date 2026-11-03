using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Soteo.Util;

public sealed class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _inner = [];

    public event Action Changed = delegate { };

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Add(item);
        Changed();
    }

    public void Clear()
    {
        _inner.Clear();
        Changed();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => _inner.Contains(item);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
        ((ICollection<KeyValuePair<TKey, TValue>>)_inner).CopyTo(array, arrayIndex);

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        bool result = ((ICollection<KeyValuePair<TKey, TValue>>)_inner).Remove(item);
        if (result) Changed();
        return result;
    }

    public int Count => _inner.Count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        _inner.Add(key, value);
        Changed();
    }

    public bool ContainsKey(TKey key) => _inner.ContainsKey(key);

    public bool Remove(TKey key)
    {
        bool result = _inner.Remove(key);
        if (result) Changed();
        return result;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) =>
        _inner.TryGetValue(key, out value);

    public TValue this[TKey key]
    {
        get => _inner[key];
        set
        {
            _inner[key] = value;
            Changed();
        }
    }

    public ICollection<TKey> Keys => _inner.Keys;

    public ICollection<TValue> Values => _inner.Values;
}
