using Soteo.Core.Snapshots;

namespace Soteo.Core.Packets;

public sealed record ShardSnapshotPacket : Packet
{
    public required ShardSnapshot Snapshot { get; init; }
}
