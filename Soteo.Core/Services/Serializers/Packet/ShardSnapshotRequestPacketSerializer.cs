using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketSerializers;

public sealed class ShardSnapshotRequestPacketSerializer(ISerializationHelper s) :
    EmptyPacketSerializer<ShardSnapshotRequestPacket>(s);
