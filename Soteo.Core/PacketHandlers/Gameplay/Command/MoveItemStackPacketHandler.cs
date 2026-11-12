using Soteo.Core.Commands;

namespace Soteo.Core.PacketHandlers.Gameplay.Command;

public sealed class MoveItemStackPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<MoveItemStackPacket, MoveItemStackCommand>(entityManager, synchronizedCampaignStateRepo);
