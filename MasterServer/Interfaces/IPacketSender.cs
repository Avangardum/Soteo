using Soteo.Shared.Attributes;
using Soteo.Shared.Packets.Master;
using Soteo.Shared.Packets.Shared;

namespace Soteo.MasterServer.Interfaces;

/// <summary>
/// Sends packets to users. Thread safe.
/// </summary>
public interface IPacketSender
{
    Task SendToAsync(Packet packet, Guid receiverId);
    
    async Task RelayFromAsync(RelayedPacket packet, Guid senderId) =>
        await SendToAsync(packet with { PeerId = senderId }, packet.PeerId);
}