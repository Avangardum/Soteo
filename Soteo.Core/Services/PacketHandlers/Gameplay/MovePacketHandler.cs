using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

public sealed class MovePacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<MovePacket, MoveCommand>(entityManager, pauseRepo);
