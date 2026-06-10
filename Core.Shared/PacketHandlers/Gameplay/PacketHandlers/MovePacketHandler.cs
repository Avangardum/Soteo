using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared.Attributes;
 using Soteo.Core.Shared.Interfaces;

namespace Soteo.Core.Gameplay.PacketHandlers;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class MovePacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<MovePacket, MoveCommand>(entityManager, pauseRepo);
