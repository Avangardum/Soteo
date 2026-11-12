namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class GameplayShardSnapshotRequestPacketHandler
(
    IShardSynchronizationServer synchronizationServer,
    IFromGameplayPacketSender packetSender,
    IShardPersistenceSnapshotManager shardPersistenceSnapshotManager
) : PacketHandler<ShardSnapshotRequestPacket>
{
    protected override void Handle(ShardSnapshotRequestPacket packet, Guid senderId)
    {
        if (senderId == Const.CampaignServerId)
            HandlePersistenceSnapshotRequest();
        else
            synchronizationServer.ReceiveSnapshotRequest(senderId);
    }

    private void HandlePersistenceSnapshotRequest()
    {
        ShardSnapshot snapshot = shardPersistenceSnapshotManager.CreateSnapshot();
        var packet = new ShardSnapshotPacket { Snapshot = snapshot };
        packetSender.Send(packet, Const.CampaignServerId);
    }
}
