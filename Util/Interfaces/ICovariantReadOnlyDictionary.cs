namespace Soteo.Util.Interfaces;

public interface ICovariantReadOnlyDictionary<TKey, out TValue> : IEnumerable<IReadOnlyKeyValuePair<TKey, TValue>>
{
    bool ContainsKey(TKey key);
    TValue this[TKey key] { get; }
    IEnumerable<TKey> Keys { get; }
    IEnumerable<TValue> Value { get; }
}
