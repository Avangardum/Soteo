using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.Ping)]
public sealed record PingPacket : Packet
{
    public required Guid Id { get; init; }
    public required bool IsResponse { get; init; }
}
