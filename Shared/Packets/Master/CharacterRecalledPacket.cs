using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.Master;

[PacketType(PacketType.CharacterRecalled)]
public sealed record CharacterRecalledPacket : Packet
{
    public Guid CharacterId { get; set; }
}