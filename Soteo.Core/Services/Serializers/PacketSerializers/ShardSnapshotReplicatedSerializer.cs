using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class ShardSnapshotReplicatedSerializer(ISerializationHelper s) :
    EmptyPacketSerializer<ShardSnapshotReplicatedPacket>(s);
