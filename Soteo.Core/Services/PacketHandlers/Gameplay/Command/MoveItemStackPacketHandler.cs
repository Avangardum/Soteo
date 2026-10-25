using Soteo.Core.Dto.Commands;
using Soteo.Core.Dto.Packets.Command;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay.Command;

public sealed class MoveItemStackPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<MoveItemStackPacket, MoveItemStackCommand>(entityManager, synchronizedCampaignStateRepo);
