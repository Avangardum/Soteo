using Soteo.Shared.Attributes;
using Soteo.Shared.Packets.MasterServer;
using Soteo.Shared.Packets.Shared;

namespace Soteo.MasterServer.Interfaces;

public interface IPacketSender
{
    void SendTo(Packet packet, Guid receiverId);
    
    void RelayFrom(RelayedPacket packet, Guid senderId);
}