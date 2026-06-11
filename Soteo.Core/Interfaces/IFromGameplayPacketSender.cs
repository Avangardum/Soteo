using Soteo.Core.Packets;

namespace Soteo.Core.Interfaces;

/// <summary>
/// Sends packets from a client to a shard server, from a shard server to a client, or from any of these to the
/// campaign server through either reliable or unreliable channel. Broadcast methods send a packet to all connected
/// shard servers when called from a client or to all connected clients when called from a shard server.
/// </summary>
public interface IFromGameplayPacketSender
{
    void SendReliable(Packet packet, params IEnumerable<Guid> receiverIds);
    void SendUnreliable(Packet packet, params IEnumerable<Guid> receiverIds);
    void BroadcastReliable(Packet packet);
    void BroadcastUnreliable(Packet packet);
}
