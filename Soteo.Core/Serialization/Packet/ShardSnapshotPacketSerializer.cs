namespace Soteo.Core.Serialization.Packet;

public sealed class ShardSnapshotPacketSerializer(ISerializationHelper s) : PacketSerializer<ShardSnapshotPacket>(s)
{
    protected override void SerializeCore(ShardSnapshotPacket packet, Stream stream) =>
        s.SerializeShardSnapshot(packet.Snapshot, stream);

    protected override ShardSnapshotPacket DeserializeCore(Stream stream) =>
        new() { Snapshot = s.DeserializeShardSnapshot(stream) };
}
