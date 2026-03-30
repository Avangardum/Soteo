using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.PlayerShard;

[PacketType(PacketType.Move)]
public sealed record MovePacket : Packet
{
    
}