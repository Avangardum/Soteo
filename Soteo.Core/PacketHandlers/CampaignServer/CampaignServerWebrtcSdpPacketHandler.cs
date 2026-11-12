namespace Soteo.Core.PacketHandlers.CampaignServer;

public sealed class CampaignServerWebrtcSdpPacketHandler
(
    IFromCampaignServerPacketSender packetSender,
    IUserRepository userRepo
) : CampaignServerWebrtcPacketHandler<WebrtcSdpPacket>(packetSender, userRepo);
