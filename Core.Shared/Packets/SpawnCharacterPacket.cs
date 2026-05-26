using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketType(PacketType.SpawnCharacter)]
public sealed record SpawnCharacterPacket : RelayedPacket
{
    public Guid SpawnPointId { get; set; }
}
