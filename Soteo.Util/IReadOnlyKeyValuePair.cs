namespace Soteo.Util;

public interface IReadOnlyKeyValuePair<out TKey, out TValue>
{
    TKey Key { get; }
    TValue Value { get; }
}
