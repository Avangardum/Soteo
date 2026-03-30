using Soteo.Shared.Packets.Shared;

namespace Soteo.Client.Interfaces;

public interface IMasterServerCommunicator : IPacketSender
{
    event Action ConnectionEstablished;
    void ConnectAsPlayer(string email, string password);
    void ConnectAsShardServer();
    void SendPacket(Packet packet);
}