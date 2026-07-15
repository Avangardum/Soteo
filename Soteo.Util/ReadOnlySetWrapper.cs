using System.Collections;
using Soteo.Util.Interfaces;

namespace Soteo.Util;

public class ReadOnlySetWrapper<T>(ISet<T> inner) : IReadOnlySet<T>
{
    public static readonly ReadOnlySetWrapper<T> Empty = new(new HashSet<T>());
    
    public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => inner.Count;

    public bool Contains(T item) => inner.Contains(item);

    public bool IsProperSubsetOf(IEnumerable<T> other) => inner.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => inner.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => inner.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => inner.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => inner.Overlaps(other);

    public bool SetEquals(IEnumerable<T> other) => inner.SetEquals(other);
}
