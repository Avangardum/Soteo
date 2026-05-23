using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Attributes;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class ShardSnapshotRequestPacketHandler(ISynchronizationServer synchronizationServer) :
    PacketHandler<ShardSnapshotRequestPacket>
{
    protected override void Handle(ShardSnapshotRequestPacket packet, Guid senderId)
    {
        synchronizationServer.ReceiveSnapshotRequest(senderId);
    }
}
