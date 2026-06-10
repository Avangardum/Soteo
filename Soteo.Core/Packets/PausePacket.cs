using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.Pause)]
public record PausePacket : Packet
{
    public required bool Pause { get; init; }
}
