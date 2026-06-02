using System.Diagnostics.CodeAnalysis;

namespace Soteo.Util.Extensions;

public static class SysObjectExtensions
{
    // NotNull here means that if Required returns, self is guaranteed to be not null
    extension<T> ([NotNull] T? self) where T : class
    {
        public T Required => self ?? throw new NullReferenceException();
    }
    
    extension<T> ([NotNull] T? self) where T : struct
    {
        public T Required => self ?? throw new NullReferenceException();
    }
    
    extension<T> (T self)
    {
        public TResult PassTo<TResult>(Func<T, TResult> func) => func(self);
        
        public void PassTo(Action<T> func) => func(self);
        
        public T Also(Action<T> func)
        {
            func(self);
            return self;
        }
        
        public T Also<TResult>(Func<T, TResult> func)
        {
            func(self);
            return self;
        }
    }
}
