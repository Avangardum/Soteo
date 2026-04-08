using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface IMasterServerCommunicator
{
    event Action ConnectionEstablished;
    void ConnectAsPlayer(string email, string password);
    void ConnectAsShardServer();
    void SendPacket(Packet packet);
}