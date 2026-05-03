namespace Soteo.Shared;

public interface IReadOnlyKeyValuePair<out TKey, out TValue>
{
    TKey Key { get; }
    TValue Value { get; }
}