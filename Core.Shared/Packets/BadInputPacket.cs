using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.BadInput)]
public sealed record BadInputPacket : Packet
{
    public string Reason { get; set; } = "";
}
