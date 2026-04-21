using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Soteo.Shared;

namespace Soteo.Gameplay;

public static class Scalable
{
    public static Scalable<T> Create<T>(params ReadOnlySpan<T> values) => new(values);
}

/// <summary>
/// Value that scales with level
/// </summary>
[CollectionBuilder(typeof(Scalable), nameof(Scalable.Create))]
public sealed class Scalable<T> : IEnumerable<T>
{
    private readonly ImmutableArray<T> _values;
    
    public Scalable(ReadOnlySpan<T> values)
    {
        _values = [..values];
    }
    
    public T this[int level] => level > _values.Length ? _values[^1] : _values[level - 1];
    
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)_values).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_values).GetEnumerator();
    }
    
    public static implicit operator Scalable<T>(T value) => [value];
}