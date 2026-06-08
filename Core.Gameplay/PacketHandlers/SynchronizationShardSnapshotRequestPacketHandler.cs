using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;

namespace Soteo.Core.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class SynchronizationShardSnapshotRequestPacketHandler(ISynchronizationServer synchronizationServer) :
    PacketHandler<SynchronizationShardSnapshotRequestPacket>
{
    protected override void Handle(SynchronizationShardSnapshotRequestPacket packet, Guid senderId)
    {
        synchronizationServer.ReceiveSnapshotRequest(senderId);
    }
}
