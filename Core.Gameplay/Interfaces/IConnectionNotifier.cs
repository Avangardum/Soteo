namespace Soteo.Gameplay.Interfaces;

public interface IConnectionNotifier
{
    event Action<Guid> PeerConnected;
    event Action<Guid> PeerDisconnected;
}
