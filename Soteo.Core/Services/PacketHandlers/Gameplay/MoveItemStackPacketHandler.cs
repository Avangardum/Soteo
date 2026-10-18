using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

public sealed class MoveItemStackPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<MoveItemStackPacket, MoveItemStackCommand>(entityManager, synchronizedCampaignStateRepo);
