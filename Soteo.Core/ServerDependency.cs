namespace Soteo.Core;

public static class ServerDependency
{
    public static ServerDependency<T> From<T>(T value) where T : class =>
        new ServerDependency<T>.NotNull(value);
    
    public static ServerDependency<T> Null<T>() where T : class =>
        new ServerDependency<T>.Null();
}

public abstract class ServerDependency<T> where T : class
{
    public abstract T? Value { get; }
    public T Required => Value.Required;
    
    public sealed class Null : ServerDependency<T>
    {
        public override T? Value => null;
    }
    
    public sealed class NotNull(T value) : ServerDependency<T>
    {
        public override T Value => value;
    }
}
