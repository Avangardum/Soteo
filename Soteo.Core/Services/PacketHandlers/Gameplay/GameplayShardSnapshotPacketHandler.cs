using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;
using Soteo.Core.SidedDependencies;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class GameplayShardSnapshotPacketHandler
(
    ServerDependency<IShardPersistenceSnapshotManager> persistenceSnapshotManager,
    ClientDependency<ISynchronizationClient> synchronizationClient
) : PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId)
    {
        persistenceSnapshotManager.Value?.ReplicateSnapshot(packet.Snapshot);
        synchronizationClient.Value?.ReceiveShardSnapshotPacket(packet);
    }
}
