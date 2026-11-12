using Soteo.Core.Commands;

namespace Soteo.Core.PacketHandlers.Gameplay.Command;

public sealed class MovePacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<MovePacket, MoveCommand>(entityManager, synchronizedCampaignStateRepo);
