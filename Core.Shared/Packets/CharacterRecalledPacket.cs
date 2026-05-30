using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.CharacterRecalled)]
public sealed record CharacterRecalledPacket : Packet
{
    public Guid CharacterId { get; set; }
}
