using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets.Command;
using Soteo.Core.Entities;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay.Command;

[GameplayPacketHandler]
[AllowClientPackets]
public abstract class CommandPacketHandler<TPacket, TCommand>
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : PacketHandler<TPacket> where TPacket : CommandPacket<TCommand> where TCommand : ICommand
{
    protected override void Handle(TPacket packet, Guid senderId)
    {
        if (synchronizedCampaignStateRepo.State.IsPaused) return;
        Unit? unit = entityManager.GetEntity<Unit>(packet.UnitId);
        if (unit != null && unit.ControllingPlayerIds.Contains(senderId))
            unit.SetCommand(packet.Command);
    }
}
