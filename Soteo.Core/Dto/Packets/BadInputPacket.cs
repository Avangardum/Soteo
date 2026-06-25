using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.BadInput)]
public sealed record BadInputPacket : Packet
{
    public required string Reason { get; set; }
}
