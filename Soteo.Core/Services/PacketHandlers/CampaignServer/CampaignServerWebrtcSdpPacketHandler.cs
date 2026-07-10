using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

public sealed class CampaignServerWebrtcSdpPacketHandler
(
    IFromCampaignServerPacketSender packetSender,
    IUserRepository userRepo
) : CampaignServerWebrtcPacketHandler<WebrtcSdpPacket>(packetSender, userRepo);
