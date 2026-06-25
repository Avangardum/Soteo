using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public abstract class CommandPacketHandler<TPacket, TCommand>
(
    IEntityManager entityManager,
    IPauseRepository pauseRepo
) : PacketHandler<TPacket> where TPacket : CommandPacket<TCommand> where TCommand : ICommand
{
    protected override void Handle(TPacket packet, Guid senderId)
    {
        if (pauseRepo.Paused) return;
        ICommandableUnit? unit = entityManager.GetEntity<ICommandableUnit>(packet.UnitId);
        if (unit != null && unit.ControllingPlayerIds.Contains(senderId))
            unit.SetCommand(packet.Command);
    }
}
