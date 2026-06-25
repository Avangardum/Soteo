using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.Move)]
public sealed record MovePacket : CommandPacket<MoveCommand>;
