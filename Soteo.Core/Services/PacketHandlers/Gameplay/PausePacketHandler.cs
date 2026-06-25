using Soteo.Core.Attributes;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class PausePacketHandler(IPauseRepository pauseRepo) : PacketHandler<PausePacket>
{
    protected override void Handle(PausePacket packet, Guid senderId)
    {
        pauseRepo.Paused = packet.Pause;
    }
}
