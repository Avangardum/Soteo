using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Dto.Snapshots;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshot)]
public sealed record ShardSnapshotPacket : Packet
{
    public required ShardSnapshot Snapshot { get; init; }
}
