using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.CampaignServerHandshake)]
public sealed record CampaignServerHandshakePacket : Packet
{
    public required string Token { get; init; }
    public required string Version { get; init; }
}
