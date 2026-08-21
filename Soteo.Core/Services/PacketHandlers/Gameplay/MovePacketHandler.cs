using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

public sealed class MovePacketHandler
(
    IEntityManager entityManager, ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<MovePacket, MoveCommand>(entityManager, synchronizedCampaignStateRepo);
