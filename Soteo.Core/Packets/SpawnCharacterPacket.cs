namespace Soteo.Core.Packets;

public sealed record SpawnCharacterPacket : RelayedPacket
{
    public required Guid CharacterId { get; init; }
}
