namespace Soteo.Core.Networking;

public interface IConnectionNotifier
{
    event Action<Guid> PeerConnected;
    event Action<Guid> PeerDisconnected;
}
