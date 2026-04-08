using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.WebrtcSdp)]
public sealed record WebrtcSdpPacket : RelayedPacket
{
    public string Sdp { get; set; } = "";
}