using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.CharacterRecalled)]
public sealed record CharacterRecalledPacket : Packet
{
    public required Guid CharacterId { get; init; }
}
