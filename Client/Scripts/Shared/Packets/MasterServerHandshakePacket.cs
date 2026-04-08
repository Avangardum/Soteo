using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.MasterServerHandshake)]
public sealed record MasterServerHandshakePacket : Packet
{
    public string Token { get; set; } = "";
    public string Version { get; set; } = "";
}