using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class StopPacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<StopPacket, StopCommand>(entityManager, pauseRepo);
