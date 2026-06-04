using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared.Attributes;

namespace Soteo.Core.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class MovePacketHandler(IEntityManager entityManager) :
    CommandPacketHandler<MovePacket, MoveCommand>(entityManager);
