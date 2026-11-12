namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public class BadInputPacketHandler : PacketHandler<BadInputPacket>
{
    protected override void Handle(BadInputPacket packet, Guid senderId)
    {
        var message = $"Received {nameof(BadInputPacket)} from {senderId} with reason:\n{packet.Reason}";
        throw new BadPacketException(message);
    }
}
