using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer(ISerializationHelper s) : PacketSerializer<ShardSnapshotPacket>(s)
{
    protected override void SerializeInternal(ShardSnapshotPacket packet, Stream stream) =>
        s.SerializeShardSnapshot(packet.Snapshot, stream);

    protected override ShardSnapshotPacket DeserializeInternal(Stream stream) =>
        new() { Snapshot = s.DeserializeShardSnapshot(stream) };
}
