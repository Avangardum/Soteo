using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer(ISerializationHelper s) : PacketSerializer<ShardSnapshotPacket>(s)
{
    protected override void SerializeInternal(ShardSnapshotPacket packet, Stream stream) =>
        s.SerializeShardSnapshot(packet.Snapshot, stream);

    protected override ShardSnapshotPacket DeserializeInternal(Stream stream) =>
        new() { Snapshot = s.DeserializeShardSnapshot(stream) };
}
