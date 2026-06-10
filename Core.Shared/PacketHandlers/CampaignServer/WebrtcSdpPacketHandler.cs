using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class WebrtcSdpPacketHandler(IPacketSender packetSender, IUserRepository userRepo) :
    WebrtcPacketHandler<WebrtcSdpPacket>(packetSender, userRepo);
