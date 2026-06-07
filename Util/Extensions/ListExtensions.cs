namespace Soteo.Util.Extensions;

public static class ListExtensions
{
    extension<T> (IReadOnlyList<T> self)
    {
        public int IndexOf(T item)
        {
            for (int i = 0; i < self.Count; i++)
                if (Equals(self[i], item))
                    return i;
            
            return -1;
        }
    }
}
