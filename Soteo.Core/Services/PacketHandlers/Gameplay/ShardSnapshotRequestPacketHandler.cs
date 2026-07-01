using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class ShardSnapshotRequestPacketHandler(ISynchronizationServer synchronizationServer) :
    PacketHandler<ShardSnapshotRequestPacket>
{
    protected override void Handle(ShardSnapshotRequestPacket packet, Guid senderId)
    {
        //Console.WriteLine($"Snapshot request from {senderId}");
        synchronizationServer.ReceiveSnapshotRequest(senderId);
    }
}
