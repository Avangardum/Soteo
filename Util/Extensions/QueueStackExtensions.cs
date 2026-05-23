namespace Soteo.Util.Extensions;

public static class QueueStackExtensions
{
    extension<T> (Queue<T> self)
    {
        public T? PeekOrDefault() => self.Count == 0 ? default : self.Peek();
    }
    
    extension<T> (Stack<T> self)
    {
        public T? PeekOrDefault() => self.Count == 0 ? default : self.Peek();
    }
}