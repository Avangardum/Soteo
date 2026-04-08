using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.MasterServer.Interfaces;

public interface IPacketSender
{
    void SendTo(Packet packet, Guid receiverId);
    
    void RelayFrom(RelayedPacket packet, Guid senderId);
}