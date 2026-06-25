using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class ShardSnapshotRequestPacketSerializer(ISerializationHelper s) :
    EmptyPacketSerializer<ShardSnapshotRequestPacket>(s);
