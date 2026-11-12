namespace Soteo.Core.Packets;

public sealed record WebrtcSdpPacket : RelayedPacket
{
    public required string Sdp { get; init; }
}
