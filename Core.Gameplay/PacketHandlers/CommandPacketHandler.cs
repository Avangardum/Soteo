using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;

namespace Soteo.Core.Gameplay.PacketHandlers;

public abstract class CommandPacketHandler<TPacket, TCommand>(IEntityManager entityManager) : PacketHandler<TPacket>
    where TPacket : CommandPacket<TCommand> where TCommand : ICommand
{
    protected override void Handle(TPacket packet, Guid senderId)
    {
        entityManager.GetEntity<Unit>(packet.UnitId)?.SetCommand(packet.Command); // todo interface, validate senderId
    }
}
