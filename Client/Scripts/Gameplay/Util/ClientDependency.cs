namespace Soteo.Gameplay.Util;

public static class ClientDependency
{
    public static ClientDependency<T> From<T>(T? value) where T : class => new(value);
}

public class ClientDependency<T> where T : class
{
    public ClientDependency() { }
    
    public ClientDependency(T? value)
    {
        Value = value;
    }
    
    public T? Value
    {
        get
        {
            if (IsServer) return null;
            return field ?? throw new InvalidOperationException($"Missing client dependency: {typeof(T)}");
        }
    }
}