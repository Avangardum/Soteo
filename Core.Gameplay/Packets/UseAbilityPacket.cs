using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketTypeCode(PacketTypeCode.UseAbility)]
public sealed record UseAbilityPacket : Packet
{
    public required UseAbilityCommand Command { get; init; }
}
