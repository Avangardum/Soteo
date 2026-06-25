using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.PacketHandlers.CampaignServer;

public sealed class WebrtcSdpPacketHandler(IFromCampaignServerPacketSender packetSender, IUserRepository userRepo) :
    WebrtcPacketHandler<WebrtcSdpPacket>(packetSender, userRepo);
