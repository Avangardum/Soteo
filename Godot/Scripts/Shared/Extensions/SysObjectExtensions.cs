using System.Diagnostics.CodeAnalysis;

namespace Soteo.Shared.Extensions;

public static class SysObjectExtensions
{
    // NotNull here means that if Required returns, self is guaranteed to be not null
    extension<T> ([NotNull] T? self) where T : class
    {
        public T Required => self ?? throw new NullReferenceException();
    }
    
    extension<T> (T self)
    {
        public TResult PassTo<TResult>(Func<T, TResult> func) => func(self);
        
        public void PassTo(Action<T> func) => func(self);
    }
}
