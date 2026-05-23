namespace Soteo.Shared;

public class ReadOnlyKeyValuePairWrapper<TKey, TValue>(KeyValuePair<TKey, TValue> pair) :
    IReadOnlyKeyValuePair<TKey, TValue>
{
    public TKey Key => pair.Key;
    public TValue Value => pair.Value;
}