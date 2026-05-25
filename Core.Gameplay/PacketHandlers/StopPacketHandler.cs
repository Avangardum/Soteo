using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;

namespace Soteo.Core.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class StopPacketHandler(IEntityManager entityManager) : PacketHandler<StopPacket>
{
    protected override void Handle(StopPacket packet, Guid senderId)
    {
        entityManager.GetEntity<Unit>(senderId)?.Stop();
    }
}
