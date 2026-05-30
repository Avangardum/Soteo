using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.Ping)]
public sealed record PingPacket : Packet
{
    public Guid Id { get; set; }
    public bool IsResponse { get; set; }
}