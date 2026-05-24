namespace Soteo.Util.Interfaces;

public interface IReadOnlyKeyValuePair<out TKey, out TValue>
{
    TKey Key { get; }
    TValue Value { get; }
}