using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.PlayerShardServer;

[PacketType(PacketType.Move)]
public sealed record MovePacket : Packet
{
    
}