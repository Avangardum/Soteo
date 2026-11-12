namespace Soteo.Core.Packets;

public sealed record CampaignServerHandshakePacket : Packet
{
    public required string Token { get; init; }
    public required string Version { get; init; }
}
