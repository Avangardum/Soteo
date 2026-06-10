using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.Ping)]
public sealed record PingPacket : Packet
{
    public required Guid Id { get; init; }
    public required bool IsResponse { get; init; }
}
