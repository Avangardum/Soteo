using Soteo.Core.Gameplay.Dto.Snapshots;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshot)]
public sealed record ShardSnapshotPacket : Packet
{
    public long Tick { get; set; }
    public ShardSnapshot Snapshot { get; set; } = null!;
}
