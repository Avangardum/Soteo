using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class ShardSnapshotReplicatedSerializer(ISerializationHelper s) :
    EmptyPacketSerializer<ShardSnapshotReplicatedPacket>(s);
