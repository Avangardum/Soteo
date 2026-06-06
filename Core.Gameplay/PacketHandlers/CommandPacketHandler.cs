using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Interfaces;

namespace Soteo.Core.Gameplay.PacketHandlers;

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
