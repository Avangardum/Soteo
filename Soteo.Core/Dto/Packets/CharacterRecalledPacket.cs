using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

public sealed record CharacterRecalledPacket : Packet
{
    public required Guid CharacterId { get; init; }
}
