using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class StopPacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<StopPacket, StopCommand>(entityManager, pauseRepo);
