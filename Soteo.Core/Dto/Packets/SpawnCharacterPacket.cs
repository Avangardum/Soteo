using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.SpawnCharacter)]
public sealed record SpawnCharacterPacket : RelayedPacket
{
    public required Guid CharacterId { get; init; }
}
