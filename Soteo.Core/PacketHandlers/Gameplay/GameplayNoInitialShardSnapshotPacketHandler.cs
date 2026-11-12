namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class GameplayNoInitialShardSnapshotPacketHandler
(
    IGameplayInitPacketReceiver receiver
) : PacketHandler<NoInitialShardSnapshotPacket>
{
    protected override void Handle(NoInitialShardSnapshotPacket packet, Guid senderId)
    {
        receiver.ReceiveNoInitialSnapshotPacket();
    }
}
