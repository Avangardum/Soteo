using System.Numerics;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.Move)]
public sealed record MovePacket : Packet
{
    public Vector2 Position { get; set; }
}