using Soteo.Util;
using Soteo.Util.Extensions;

namespace Soteo.Gameplay.Util;

public static class ServerDependency
{
    public static ServerDependency<T> From<T> (T? value) where T : class => new(value);
}

public class ServerDependency<T> where T : class
{
    public ServerDependency() { }
    
    public ServerDependency(T? value)
    {
        Value = value;
    }
    
    public T? Value
    {
        get
        {
            if (!Const.IsServer) return null;
            return field ?? throw new InvalidOperationException($"Missing server dependency: {typeof(T)}");
        }
        private set;
    }
    
    public T Required => Value.Required;
}