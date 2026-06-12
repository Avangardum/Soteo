using Soteo.Core.Attributes;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public sealed class PausePacketHandler(IPauseRepository pauseRepo) : PacketHandler<PausePacket>
{
    protected override void Handle(PausePacket packet, Guid senderId)
    {
        pauseRepo.Paused = packet.Pause;
    }
}
