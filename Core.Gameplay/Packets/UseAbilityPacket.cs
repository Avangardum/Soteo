using Soteo.Gameplay.Commands;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.UseAbility)]
public sealed record UseAbilityPacket : Packet
{
    public UseAbilityCommand Command { get; set; } = null!;
}