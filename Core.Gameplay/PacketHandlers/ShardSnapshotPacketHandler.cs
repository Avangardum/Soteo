using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.PacketHandlers;

public sealed class ShardSnapshotPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<SynchronizationShardSnapshotPacket>
{
    protected override void Handle(SynchronizationShardSnapshotPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotPacket(packet);
    }
}
