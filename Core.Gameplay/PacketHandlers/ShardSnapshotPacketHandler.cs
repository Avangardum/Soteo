using Soteo.CampaignServer.PacketHandlers;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

public sealed class ShardSnapshotPacketHandler(ISynchronizationClient synchronizationClient) :
    PacketHandler<ShardSnapshotPacket>
{
    protected override void Handle(ShardSnapshotPacket packet, Guid senderId)
    {
        synchronizationClient.ReceiveShardSnapshotPacket(packet);
    }
}