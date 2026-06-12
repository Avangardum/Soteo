using Soteo.Core.Dto;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketSerializers;

public sealed class ShardSnapshotPacketSerializer(ISerializationHelper s) : PacketSerializer<ShardSnapshotPacket>(s)
{
    protected override void SerializeInternal(ShardSnapshotPacket packet, Stream stream)
    {
        s.SerializeLong(packet.Snapshot.Tick, stream);
        s.SerializeIndexedDictionary(packet.Snapshot.Entities, s.SerializeEntitySnapshot, stream);
    }
    
    protected override ShardSnapshotPacket DeserializeInternal(Stream stream)
    {
        return new ShardSnapshotPacket
        {
            Snapshot = new ShardSnapshot
            {
                Tick = s.DeserializeLong(stream),
                Entities = s.DeserializeIndexedDictionary(s.DeserializeEntitySnapshot, it => it.Id, stream),
            },
        };
    }
}
