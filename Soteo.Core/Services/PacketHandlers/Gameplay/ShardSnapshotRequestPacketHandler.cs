using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class ShardSnapshotRequestPacketHandler(ISynchronizationServer synchronizationServer) :
    PacketHandler<ShardSnapshotRequestPacket>
{
    protected override void Handle(ShardSnapshotRequestPacket packet, Guid senderId)
    {
        synchronizationServer.ReceiveSnapshotRequest(senderId);
    }
}
