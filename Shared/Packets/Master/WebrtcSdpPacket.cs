using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets.Master;

[PacketType(PacketType.WebrtcSdp)]
public sealed record WebrtcSdpPacket : RelayedPacket
{
    public string SdpType { get; set; } = "";
    public string Sdp { get; set; } = "";
}