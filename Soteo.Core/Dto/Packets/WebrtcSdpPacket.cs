using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

public sealed record WebrtcSdpPacket : RelayedPacket
{
    public required string Sdp { get; init; }
}
