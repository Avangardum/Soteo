using Soteo.Core.Commands;

namespace Soteo.Core.PacketHandlers.Gameplay.Command;

public sealed class EquipPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<EquipPacket, EquipCommand>(entityManager, synchronizedCampaignStateRepo);
