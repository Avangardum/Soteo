using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.Ping)]
public sealed record PingPacket : Packet
{
    public Guid Id { get; set; }
    public bool IsResponse { get; set; }
}