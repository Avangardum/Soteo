using Soteo.Shared.Packets.Shared;

namespace Soteo.Client;

public interface IPacketSender
{
    void SendReliable(Packet packet, Guid receiverId);
}