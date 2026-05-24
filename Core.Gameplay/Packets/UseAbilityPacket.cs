using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketType(PacketType.UseAbility)]
public sealed record UseAbilityPacket : Packet
{
    public UseAbilityCommand Command { get; set; } = null!;
}