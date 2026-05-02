namespace Soteo.Shared.Extensions;

public static class ArrayExtensions
{
    extension<T> (T[] self)
    {
        public T RingGet(long i) => self[Maths.PosMod(i, self.Length)];
        
        public void RingSet(long i, T value) => self[Maths.PosMod(i, self.Length)] = value;
    }
}