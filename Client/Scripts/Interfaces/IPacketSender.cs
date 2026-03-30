using Soteo.Shared.Packets.Shared;

namespace Soteo.Client.Interfaces;

public interface IPacketSender
{
    void SendReliable(Packet packet, Guid receiverId);
}