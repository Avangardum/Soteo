using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class GameplayNoInitialShardSnapshotPacketHandler
(
    IGameplayInitPacketReceiver receiver
) : PacketHandler<NoInitialShardSnapshotPacket>
{
    protected override void Handle(NoInitialShardSnapshotPacket packet, Guid senderId)
    {
        receiver.ReceiveNoInitialSnapshotPacket();
    }
}
