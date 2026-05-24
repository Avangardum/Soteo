using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.PacketHandlers;

public sealed class ShardSnapshotDeltaPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotDeltaPacket>
{
    protected override void Handle(ShardSnapshotDeltaPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotDeltaPacket(packet);
    }
}