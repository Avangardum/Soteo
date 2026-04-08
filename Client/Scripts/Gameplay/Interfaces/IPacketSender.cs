using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface IPacketSender
{
    void SendReliable(Packet packet, Guid receiverId);
    void SendUnreliable(Packet packet, Guid receiverId);
    void BroadcastReliable(Packet packet);
    void BroadcastUnreliable(Packet packet);
}