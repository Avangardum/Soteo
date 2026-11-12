namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class GameplayShardSnapshotPacketHandler
(
    ServerDependency<IShardPersistenceSnapshotManager> persistenceSnapshotManager,
    ClientDependency<IShardSynchronizationClient> synchronizationClient
) : PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId)
    {
        persistenceSnapshotManager.Value?.ReplicateSnapshot(packet.Snapshot);
        synchronizationClient.Value?.ReceiveShardSnapshotPacket(packet);
    }
}
