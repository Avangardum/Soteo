using Soteo.Core.Commands;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.PacketHandlers.Gameplay.Command;

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
