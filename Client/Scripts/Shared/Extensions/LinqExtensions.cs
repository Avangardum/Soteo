namespace Soteo.Shared.Extensions;

public static class LinqExtensions
{
    extension<T> (IEnumerable<T?> self)
    {
        public IEnumerable<T> WhereNotNull() => self.Where(it => it != null)!;
    }
}