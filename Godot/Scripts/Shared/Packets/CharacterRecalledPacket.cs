using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.CharacterRecalled)]
public sealed record CharacterRecalledPacket : Packet
{
    public Guid CharacterId { get; set; }
}