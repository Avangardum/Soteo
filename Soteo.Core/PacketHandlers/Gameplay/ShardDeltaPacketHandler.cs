namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class ShardDeltaPacketHandler(IShardSynchronizationClient synchronizationClient) :
    PacketHandler<ShardDeltaPacket>
{
    protected override void Handle(ShardDeltaPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardDeltaPacket(packet);
    }
}
