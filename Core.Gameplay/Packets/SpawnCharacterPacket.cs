using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.SpawnCharacter)]
public sealed record SpawnCharacterPacket : RelayedPacket
{
    public Guid SpawnPointId { get; set; }
}