using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Interfaces;

/// <summary>
/// Sends packets from clients and shard servers
/// </summary>
public interface IFromGameplayPacketSender
{
    /// <summary>
    /// Send a packet
    /// </summary>
    /// <param name="reliable">
    /// When true, sends the packet using the reliable channel with guaranteed delivery in the correct order.
    /// When false, sends the packet using the unreliable channel that can lose or reorder packets, but can be faster,
    /// because it doesn't wait for previous packets to be delivered.
    /// </param>
    /// <param name="deferToNextTick">
    /// When true, the packet is deferred until the next tick. Deferred packets are sent very early in a tick, before
    /// most of the tick processing happens. Use this when it's important to send packets in a steady rhythm,
    /// independent of time it takes to process a tick, such as in synchronization.
    /// </param>
    void Send(Packet packet, IReadOnlyList<Guid> receiverIds, bool reliable = true, bool deferToNextTick = false);

    /// <inheritdoc
    ///     cref="Send(Soteo.Core.Dto.Packets.Packet,System.Collections.Generic.IReadOnlyList{System.Guid},bool,bool)"
    /// />
    void Send(Packet packet, Guid receiverId, bool reliable = true, bool deferToNextTick = false) =>
        Send(packet, [receiverId], reliable, deferToNextTick);

    /// <summary>
    /// Send a packet to all connected players (if called by a shard server) or to all connected shard servers
    /// (if called by a player).
    /// </summary>
    /// <inheritdoc
    ///     cref="Send(Soteo.Core.Dto.Packets.Packet,System.Collections.Generic.IReadOnlyList{System.Guid},bool,bool)"
    /// />
    void Broadcast(Packet packet, bool reliable = true, bool deferToNextTick = false);
}
