using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.Master;

[PacketType(PacketType.BadInput)]
public sealed record BadInputPacket : Packet
{
    public string Reason { get; set; } = "";
}