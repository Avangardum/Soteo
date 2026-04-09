namespace Soteo.Shared.Extensions;

public static class ArrayExtensions
{
    extension<T> (T[] self)
    {
        public T RingGet(long i) => self[SoteoMath.PosMod(i, self.Length)];
        
        public void RingSet(long i, T value) => self[SoteoMath.PosMod(i, self.Length)] = value;
    }
}