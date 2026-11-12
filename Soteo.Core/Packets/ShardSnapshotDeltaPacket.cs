namespace Soteo.Core.Packets;

public sealed record ShardSnapshotDeltaPacket : Packet
{
    public required double ServerLoad { get; init; }
    public required ShardSnapshotDelta SnapshotDelta { get; init; }
}
