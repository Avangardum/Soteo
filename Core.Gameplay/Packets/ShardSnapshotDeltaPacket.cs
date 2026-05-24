using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketType(PacketType.ShardSnapshotDelta)]
public sealed record ShardSnapshotDeltaPacket : Packet
{
    public long Tick { get; set; }
    public double ServerLoad { get; set; }
    public ShardSnapshotDelta SnapshotDelta { get; set; } = null!;
}