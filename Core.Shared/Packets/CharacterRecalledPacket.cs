using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketType(PacketType.CharacterRecalled)]
public sealed record CharacterRecalledPacket : Packet
{
    public Guid CharacterId { get; set; }
}