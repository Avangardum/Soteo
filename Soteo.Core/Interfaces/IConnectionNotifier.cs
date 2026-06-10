namespace Soteo.Core.Interfaces;

public interface IConnectionNotifier
{
    event Action<Guid> PeerConnected;
    event Action<Guid> PeerDisconnected;
}
