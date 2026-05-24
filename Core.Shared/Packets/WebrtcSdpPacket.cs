using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketType(PacketType.WebrtcSdp)]
public sealed record WebrtcSdpPacket : RelayedPacket
{
    public string Sdp { get; set; } = "";
}