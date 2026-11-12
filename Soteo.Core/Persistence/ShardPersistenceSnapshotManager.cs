namespace Soteo.Core.Persistence;

public sealed class ShardPersistenceSnapshotManager
(
    IEntitySnapshotManager entitySnapshotManager,
    ICurrentTickRepository tickRepo,
    IFromGameplayPacketSender packetSender
) : IShardPersistenceSnapshotManager
{
    public event Action SnapshotReplicated = delegate { };

    public ShardSnapshot CreateSnapshot()
    {
        return new ShardSnapshot
        {
            Tick = tickRepo.Tick,
            Entities = entitySnapshotManager.CreateEntitySnapshots(),
        };
    }

    public void ReplicateSnapshot(ShardSnapshot snapshot)
    {
        tickRepo.Tick = snapshot.Tick;
        entitySnapshotManager.ReplicateEntitySnapshots(snapshot.Entities);
        packetSender.Send(new ShardSnapshotReplicatedPacket(), Const.CampaignServerId);
        SnapshotReplicated();
    }
}
