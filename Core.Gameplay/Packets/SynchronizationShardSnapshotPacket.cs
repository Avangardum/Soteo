using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketTypeCode(PacketTypeCode.SynchronizationShardSnapshot)]
public sealed record SynchronizationShardSnapshotPacket : Packet
{
    public required long Tick { get; init; }
    public required SynchronizationShardSnapshot Snapshot { get; init; }
}
