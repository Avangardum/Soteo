using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketTypeCode(PacketTypeCode.SynchronizationShardSnapshotDelta)]
public sealed record SynchronizationShardSnapshotDeltaPacket : Packet
{
    public required long Tick { get; init; }
    public required double ServerLoad { get; init; }
    public required SynchronizationShardSnapshotDelta SnapshotDelta { get; init; }
}
