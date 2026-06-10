using Soteo.Core.Attributes;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshot)]
public sealed record ShardSnapshotPacket : Packet
{
    public required ShardSnapshot Snapshot { get; init; }
}
