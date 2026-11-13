using Soteo.Core.Commands;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.PacketHandlers.Gameplay.Command;

public sealed class StopPacketHandler
(
    IEntityManager entityManager,
    ISynchronizedCampaignStatePuppetRepository synchronizedCampaignStateRepo
) : CommandPacketHandler<StopPacket, StopCommand>(entityManager, synchronizedCampaignStateRepo);
