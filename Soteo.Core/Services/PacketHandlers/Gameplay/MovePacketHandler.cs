using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class MovePacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<MovePacket, MoveCommand>(entityManager, pauseRepo);
