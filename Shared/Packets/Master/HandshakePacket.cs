using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.Master;

[PacketType(PacketType.Handshake)]
public sealed record HandshakePacket : Packet
{
    public string Token { get; set; } = "";
    public string Version { get; set; } = "";
}