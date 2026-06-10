using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshotDelta)]
public sealed record ShardSnapshotDeltaPacket : Packet
{
    public required double ServerLoad { get; init; }
    public required ShardSnapshotDelta SnapshotDelta { get; init; }
}
