using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

public sealed record CampaignServerHandshakePacket : Packet
{
    public required string Token { get; init; }
    public required string Version { get; init; }
}
