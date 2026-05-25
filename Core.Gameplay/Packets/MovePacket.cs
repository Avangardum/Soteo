using System.Numerics;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketType(PacketType.Move)]
public sealed record MovePacket : Packet
{
    public Vector2 Position { get; set; }
}
