using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

[PacketTypeCode(PacketTypeCode.Pause)]
public record PausePacket : Packet
{
    public required bool Pause { get; init; }
}
