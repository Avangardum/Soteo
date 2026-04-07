using Soteo.Shared.Packets.Shared;

namespace Soteo.Client.Interfaces;

public interface IPacketSender
{
    void SendReliable(Packet packet, Guid receiverId);
    void SendUnreliable(Packet packet, Guid receiverId);
    void BroadcastReliable(Packet packet);
    void BroadcastUnreliable(Packet packet);
}