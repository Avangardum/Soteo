using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

public sealed class CampaignServerWebrtcIceCandidatePacketHandler
(
    IFromCampaignServerPacketSender packetSender,
    IUserRepository userRepo
) : CampaignServerWebrtcPacketHandler<WebrtcIceCandidatePacket>(packetSender, userRepo);
