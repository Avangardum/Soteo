using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Soteo.Core.Gameplay.Dto;

public abstract class Scalable
{
    public static Scalable<T> Create<T>(params ReadOnlySpan<T> values) where T : notnull => new(values);
    
    public abstract string ToBbcode(int? highlightLevel = null, string? format = null);
}

/// <summary>
/// Value that scales with level
/// </summary>
[CollectionBuilder(typeof(Scalable), nameof(Create))]
public sealed class Scalable<T> : Scalable, IEnumerable<T> where T : notnull
{
    private readonly ImmutableArray<T> _values;
    
    public Scalable(ReadOnlySpan<T> values)
    {
        _values = [..values];
    }
    
    public T this[int level] => level > _values.Length ? _values[^1] : _values[level - 1];
    
    public IEnumerator<T> GetEnumerator() =>
        ((IEnumerable<T>)_values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable)_values).GetEnumerator();

    public static implicit operator Scalable<T>(T value) => [value];

    public override string ToString() => string.Join(" / ", this);

    public override string ToBbcode(int? highlightLevel = null, string? format = null)
    {
        return string.Join(" / ",
            this.Select((v, i) => i + 1 == highlightLevel ? $"[b]{Format(v, format)}[/b]" : Format(v, format)));
    } // todo overflow handling inconsistent with this[int]
    
    private string Format(T value, string? format) =>
        value is IFormattable f && format != null ? f.ToString(format, CultureInfo.CurrentCulture) : value.ToString();
}
