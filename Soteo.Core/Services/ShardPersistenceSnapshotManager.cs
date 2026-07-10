using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Services;

public sealed class ShardPersistenceSnapshotManager
(
    IEntitySnapshotManager entitySnapshotManager,
    ICurrentTickRepository tickRepo,
    IFromGameplayPacketSender packetSender
) : IShardPersistenceSnapshotManager
{
    public ShardSnapshot CreateSnapshot()
    {
        return new ShardSnapshot
        {
            Tick = tickRepo.Value,
            Entities = entitySnapshotManager.CreateEntitySnapshots(),
        };
    }

    public void ReplicateSnapshot(ShardSnapshot snapshot)
    {
        tickRepo.Value = snapshot.Tick;
        entitySnapshotManager.ReplicateEntitySnapshots(snapshot.Entities);
        packetSender.SendReliable(new ShardSnapshotReplicatedPacket(), Const.CampaignServerId);
    }
}
