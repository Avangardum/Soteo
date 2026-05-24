using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketType(PacketType.CampaignServerHandshake)]
public sealed record CampaignServerHandshakePacket : Packet
{
    public string Token { get; set; } = "";
    public string Version { get; set; } = "";
}