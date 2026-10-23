using Soteo.Core.Commands;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.Gameplay;

public sealed class UseAbilityPacketHandler // todo move command packet handlers to a subfolder
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<UseAbilityPacket, UseAbilityCommand>(entityManager, synchronizedCampaignStateRepo);
