using Soteo.Core.Packets;

namespace Soteo.Core.Interfaces;

public interface IFromGameplayPacketSender
{
    void SendReliable(Packet packet, params IEnumerable<Guid> receiverIds);
    void SendUnreliable(Packet packet, params IEnumerable<Guid> receiverIds);
    void BroadcastReliable(Packet packet);
    void BroadcastUnreliable(Packet packet);
}
