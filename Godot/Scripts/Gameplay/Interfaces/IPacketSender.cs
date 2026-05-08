using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface IPacketSender
{
    void SendReliable(Packet packet, Guid receiverId);
    void SendUnreliable(Packet packet, Guid receiverId);
    void SendReliable(Packet packet, IEnumerable<Guid> receiverIds);
    void SendUnreliable(Packet packet, IEnumerable<Guid> receiverIds);
    void BroadcastReliable(Packet packet);
    void BroadcastUnreliable(Packet packet);
}