using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.WebrtcSdp)]
public sealed record WebrtcSdpPacket : RelayedPacket
{
    public required string Sdp { get; init; }
}
