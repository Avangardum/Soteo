using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;

namespace Soteo.Core.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public sealed class ShardSnapshotDeltaPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotDeltaPacket>
{
    protected override void Handle(ShardSnapshotDeltaPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotDeltaPacket(packet);
    }
}
