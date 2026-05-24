using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.CampaignServerHandshake)]
public sealed record CampaignServerHandshakePacket : Packet
{
    public string Token { get; set; } = "";
    public string Version { get; set; } = "";
}