using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.SynchronizedCampaignState)]
public sealed record SynchronizedCampaignStatePacket(SynchronizedCampaignState Value) : Packet;
