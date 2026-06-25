using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

[GameplayPacketHandler]
[AllowClientPackets]
public sealed class UseAbilityPacketHandler(IEntityManager entityManager, IPauseRepository pauseRepo) :
    CommandPacketHandler<UseAbilityPacket, UseAbilityCommand>(entityManager, pauseRepo);
