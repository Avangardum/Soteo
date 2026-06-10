using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.PacketHandlers;

[GameplayPacketHandler]
public sealed class PausePacketHandler(IPauseRepository pauseRepo) : PacketHandler<PausePacket>
{
    protected override void Handle(PausePacket packet, Guid senderId)
    {
        pauseRepo.Paused = packet.Pause;
    }
}
