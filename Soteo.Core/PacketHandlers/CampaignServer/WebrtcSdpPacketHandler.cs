using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class WebrtcSdpPacketHandler(IFromCampaignServerPacketSender packetSender, IUserRepository userRepo) :
    WebrtcPacketHandler<WebrtcSdpPacket>(packetSender, userRepo);
