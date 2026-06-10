using Soteo.Core.CampaignServer.GameState.DataObjects;
using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.CampaignServer.PacketHandlers;

public sealed class WebrtcIceCandidatePacketHandler(IFromCampaignServerPacketSender packetSender, IUserRepository userRepo) :
    WebrtcPacketHandler<WebrtcIceCandidatePacket>(packetSender, userRepo);
