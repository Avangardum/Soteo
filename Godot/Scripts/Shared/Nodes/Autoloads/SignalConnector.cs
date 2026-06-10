using Soteo.Util;

namespace Soteo.Main.Shared.Nodes.Autoloads;

public sealed class SignalConnector : Node
{
    private static readonly LateInit<SignalConnector> _instance = new();
    
    public static SignalConnector Instance => _instance;
    
    public override void _Ready()
    {
        _instance.Value = this;
    }
    
    public IDisposable ConnectSignal(GdObject obj, string signal, Action handler)
    {
        var connection = new Connection(handler);
        obj.Connect(signal, connection, nameof(Connection.Handle));
        return new DelegateDisposable(connection.Free);
    }
    
    public IDisposable ConnectSignal<T>(GdObject obj, string signal, Action<T> handler)
    {
        var connection = new Connection<T>(handler);
        obj.Connect(signal, connection, nameof(Connection.Handle));
        return new DelegateDisposable(connection.Free);
    }
    
    public IDisposable ConnectSignal<T1, T2>(GdObject obj, string signal, Action<T1, T2> handler)
    {
        var connection = new Connection<T1, T2>(handler);
        obj.Connect(signal, connection, nameof(Connection.Handle));
        return new DelegateDisposable(connection.Free);
    }
    
    private sealed class Connection(Action handler) : GdObject
    {
        public void Handle() => handler();
    }
    
    private sealed class Connection<T>(Action<T> handler) : GdObject
    {
        public void Handle(T arg) => handler(arg);
    }
    
    private sealed class Connection<T1, T2>(Action<T1, T2> handler) : GdObject
    {
        public void Handle(T1 arg1, T2 arg2) => handler(arg1, arg2);
    }
}
