using Soteo.Core.Dto.Commands;
using Soteo.Core.Dto.Packets.Command;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay.Command;

public sealed class UnequipPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<UnequipPacket, UnequipCommand>(entityManager, synchronizedCampaignStateRepo);
