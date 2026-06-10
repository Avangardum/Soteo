using System.Numerics;
using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketTypeCode(PacketTypeCode.Move)]
public sealed record MovePacket : CommandPacket<MoveCommand>;
