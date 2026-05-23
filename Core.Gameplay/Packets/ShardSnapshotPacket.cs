using Soteo.Gameplay;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.ShardSnapshot)]
public sealed record ShardSnapshotPacket : Packet
{
    public long Tick { get; set; }
    public ShardSnapshot Snapshot { get; set; } = null!;
}