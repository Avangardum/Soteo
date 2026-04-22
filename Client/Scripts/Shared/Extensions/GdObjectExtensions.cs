namespace Soteo.Shared.Extensions;

public static class GdObjectExtensions
{
    extension<T> (T? self) where T : Object
    {
        public T? AsValid() => Object.IsInstanceValid(self) ? self : null;
    }
}