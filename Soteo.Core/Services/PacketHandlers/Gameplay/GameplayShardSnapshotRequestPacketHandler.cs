using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class GameplayShardSnapshotRequestPacketHandler
(
    ISynchronizationServer synchronizationServer,
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
        packetSender.SendReliable(packet, Const.CampaignServerId);
        // TODO fails with a lot of entities (ws buffer overflow)
    }
}
