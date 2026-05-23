using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class ShardSnapshotDeltaPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotDeltaPacket>
{
    protected override void Handle(ShardSnapshotDeltaPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotDeltaPacket(packet);
    }
}