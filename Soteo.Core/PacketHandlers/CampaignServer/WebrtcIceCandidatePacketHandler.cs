using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketHandlers.CampaignServer;

public sealed class WebrtcIceCandidatePacketHandler(IFromCampaignServerPacketSender packetSender, IUserRepository userRepo) :
    WebrtcPacketHandler<WebrtcIceCandidatePacket>(packetSender, userRepo);
