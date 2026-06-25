using Soteo.Core.Attributes;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshotDelta)]
public sealed record ShardSnapshotDeltaPacket : Packet
{
    public required double ServerLoad { get; init; }
    public required ShardSnapshotDelta SnapshotDelta { get; init; }
}
