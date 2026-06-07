using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay;

public static class ClientDependency
{
    public static ClientDependency<T> From<T>(T value) where T : class =>
        new ClientDependency<T>.NotNull(value);
    
    public static ClientDependency<T> Null<T>() where T : class =>
        new ClientDependency<T>.Null();
}

public abstract class ClientDependency<T> where T : class
{
    public abstract T? Value { get; }
    public T Required => Value.Required;
    
    public sealed class Null : ClientDependency<T>
    {
        public override T? Value => null;
    }
    
    public sealed class NotNull(T value) : ClientDependency<T>
    {
        public override T Value => value;
    }
}
