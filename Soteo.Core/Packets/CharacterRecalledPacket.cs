namespace Soteo.Core.Packets;

public sealed record CharacterRecalledPacket : Packet
{
    public required Guid CharacterId { get; init; }
}
