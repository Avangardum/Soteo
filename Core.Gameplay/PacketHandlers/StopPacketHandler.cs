using Soteo.Core.Gameplay.Commands;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Packets;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;

namespace Soteo.Core.Gameplay.PacketHandlers;

[AllowClientPackets]
public sealed class StopPacketHandler(IEntityManager entityManager) :
    CommandPacketHandler<StopPacket, StopCommand>(entityManager);
