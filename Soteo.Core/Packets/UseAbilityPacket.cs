using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

[PacketTypeCode(PacketTypeCode.UseAbility)]
public sealed record UseAbilityPacket : CommandPacket<UseAbilityCommand>;
