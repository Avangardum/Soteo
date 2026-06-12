using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public sealed class ShardSnapshotPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotPacket(packet);
    }
}
