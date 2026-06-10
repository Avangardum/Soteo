using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Interfaces;

namespace Soteo.Core.Gameplay.PacketHandlers;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class UseAbilityPacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<UseAbilityPacket, UseAbilityCommand>(entityManager, pauseRepo);
