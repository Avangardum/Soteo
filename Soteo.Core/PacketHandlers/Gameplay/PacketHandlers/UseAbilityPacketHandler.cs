using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.Gameplay.PacketHandlers;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class UseAbilityPacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<UseAbilityPacket, UseAbilityCommand>(entityManager, pauseRepo);
