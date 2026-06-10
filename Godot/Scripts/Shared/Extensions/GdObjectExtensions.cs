using Soteo.Main.Shared.Nodes.Autoloads;

namespace Soteo.Main.Shared.Extensions;

public static class GdObjectExtensions
{
    extension (GdObject self)
    {
        public IDisposable Connect(string signal, Action handler) =>
            SignalConnector.Instance.ConnectSignal(self, signal, handler);
        
        public IDisposable Connect<T>(string signal, Action<T> handler) =>
            SignalConnector.Instance.ConnectSignal(self, signal, handler);
        
        public IDisposable Connect<T1, T2>(string signal, Action<T1, T2> handler) =>
            SignalConnector.Instance.ConnectSignal(self, signal, handler);
    }
}
