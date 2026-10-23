using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

public sealed class UnequipPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<UnequipPacket, UnequipCommand>(entityManager, synchronizedCampaignStateRepo);
