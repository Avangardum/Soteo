using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets.Shared;

[PacketType(PacketType.BadInput)]
public sealed record BadInputPacket : Packet
{
    public string Reason { get; set; } = "";
}