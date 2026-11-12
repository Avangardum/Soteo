using Soteo.Core.Commands;

namespace Soteo.Core.PacketHandlers.Gameplay.Command;

public sealed class UseAbilityPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<UseAbilityPacket, UseAbilityCommand>(entityManager, synchronizedCampaignStateRepo);
