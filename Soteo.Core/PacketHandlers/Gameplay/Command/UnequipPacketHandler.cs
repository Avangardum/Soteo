using Soteo.Core.Commands;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.PacketHandlers.Gameplay.Command;

public sealed class UnequipPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<UnequipPacket, UnequipCommand>(entityManager, synchronizedCampaignStateRepo);
