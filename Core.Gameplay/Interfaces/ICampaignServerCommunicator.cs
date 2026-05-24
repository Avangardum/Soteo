using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface ICampaignServerCommunicator
{
    event Action ConnectionEstablished;
    void ConnectAsPlayer(string email, string password);
    void ConnectAsShardServer();
    void SendPacket(Packet packet);
}