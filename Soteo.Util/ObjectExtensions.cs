using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Soteo.Util;

public static class ObjectExtensions
{
    extension<T> ([NotNull] T? self) where T : class
    {
        public T Required([CallerArgumentExpression(nameof(self))] string expr = "") =>
            self ?? throw new Exception($"{expr} is null");
    }

    extension<T> ([NotNull] T? self) where T : struct
    {
        public T Required([CallerArgumentExpression(nameof(self))] string expr = "") =>
            self ?? throw new Exception($"{expr} is null");
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
