using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class ShardSnapshotRequestPacketSerializer(ISerializationHelper s) :
    EmptyPacketSerializer<ShardSnapshotRequestPacket>(s);
