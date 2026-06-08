using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.PacketHandlers;

public sealed class SynchronizationShardSnapshotDeltaPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<SynchronizationShardSnapshotDeltaPacket>
{
    protected override void Handle(SynchronizationShardSnapshotDeltaPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotDeltaPacket(packet);
    }
}
