namespace Soteo.Shared.Extensions;

public static class ArrayExtensions
{
    extension<T> (T[] self)
    {
        public T RingGet(long i) => self[Maths.PosMod(i, self.Length)];
        
        public void RingSet(long i, T value) => self[Maths.PosMod(i, self.Length)] = value;
        
        public void UnrollRingTo(Span<T> target, long start)
        {
            for (int i = 0; i < self.Length; i++)
                target[i] = self.RingGet(start + i);
        }
        
        public T[] UnrollRing(long start)
        {
            T[] result = new T[self.Length];
            self.UnrollRingTo(result, start);
            return result;
        }
    }
}