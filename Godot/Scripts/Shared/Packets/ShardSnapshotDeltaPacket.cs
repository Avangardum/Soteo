using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.ShardSnapshot)]
public sealed record ShardSnapshotDeltaPacket : Packet
{
    public long Tick { get; set; }
    public double ServerLoad { get; set; }
    public ShardSnapshotDelta Snapshot { get; set; } = null!;
}