using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Interfaces;

public interface ICampaignServerCommunicator
{
    event Action ConnectionEstablished;
    void ConnectAsPlayer(string email, string password);
    void ConnectAsShardServer();
    void SendPacket(Packet packet);
}