namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class ShardSnapshotDeltaPacketHandler(IShardSynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotDeltaPacket>
{
    protected override void Handle(ShardSnapshotDeltaPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotDeltaPacket(packet);
    }
}
