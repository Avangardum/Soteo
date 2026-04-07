using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.MasterServer;

[PacketType(PacketType.MasterServerHandshake)]
public sealed record MasterServerHandshakePacket : Packet
{
    public string Token { get; set; } = "";
    public string Version { get; set; } = "";
}