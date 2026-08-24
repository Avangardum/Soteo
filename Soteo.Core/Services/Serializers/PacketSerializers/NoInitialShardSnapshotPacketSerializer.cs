using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class NoInitialShardSnapshotPacketSerializer(ISerializationHelper s) :
    EmptyPacketSerializer<NoInitialShardSnapshotPacket>(s);
    