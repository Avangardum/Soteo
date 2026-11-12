namespace Soteo.Core.PacketHandlers.CampaignServer;

public sealed class CampaignServerWebrtcIceCandidatePacketHandler
(
    IFromCampaignServerPacketSender packetSender,
    IUserRepository userRepo
) : CampaignServerWebrtcPacketHandler<WebrtcIceCandidatePacket>(packetSender, userRepo);
