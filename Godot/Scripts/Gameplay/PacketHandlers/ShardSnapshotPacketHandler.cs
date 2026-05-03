using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class ShardSnapshotPacketHandler(ISynchronizationClient receiver) :
    PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId)
    {
        receiver.ReceiveShardSnapshotPacket(packet);
    }
}