namespace Soteo.Util;

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

        public int RequiredIndexOf(T item)
        {
            int result = self.IndexOf(item);
            if (result == -1)
                throw new ArgumentException($"{item} is not in the list");
            return result;
        }
    }
}
