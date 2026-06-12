using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.CampaignServer;

public sealed class WebrtcSdpPacketHandler(IFromCampaignServerPacketSender packetSender, IUserRepository userRepo) :
    WebrtcPacketHandler<WebrtcSdpPacket>(packetSender, userRepo);
