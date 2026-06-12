using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

[PacketTypeCode(PacketTypeCode.WebrtcSdp)]
public sealed record WebrtcSdpPacket : RelayedPacket
{
    public required string Sdp { get; init; }
}
