using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public sealed class ShardSnapshotDeltaPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotDeltaPacket>
{
    protected override void Handle(ShardSnapshotDeltaPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotDeltaPacket(packet);
    }
}
