using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class GameplayShardSnapshotPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotPacket(packet);
    }
}
