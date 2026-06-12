namespace Soteo.Core.Packets;

/// <summary>
/// Packet sent to the campaign server, which relays it to the target receiver
/// </summary>
public abstract record RelayedPacket : Packet
{
    /// <summary>
    /// In campaign server, inbound packets have final receiver id and outbound packets have initial sender id.
    /// In shard server / client, inbound packets have initial sender id and outbound packets have final receiver id.
    /// </summary>
    public required Guid PeerId { get; init; }
}
