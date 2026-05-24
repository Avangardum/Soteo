using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.PacketHandlers;

public sealed class ShardSnapshotPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotPacket(packet);
    }
}