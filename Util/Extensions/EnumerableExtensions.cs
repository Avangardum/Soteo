namespace Soteo.Util.Extensions;

public static class EnumerableExtensions
{
    extension<T> (IEnumerable<T> self)
    {
        public string JoinToString(string separator) => string.Join(separator, self);
        public string JoinToString(char separator) => string.Join(separator.ToString(), self);
    }
}