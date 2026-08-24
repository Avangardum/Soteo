using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

/// <summary>
/// Sent from a shard server to the campaign server to notify that it finished all initialization steps except
/// ensuring that other servers are initialized and waits for <see cref="CampaignInitializedPacket"/> to fully
/// finish initialization
/// </summary>
[PacketTypeCode(PacketTypeCode.ShardServerInitAwaitingCampaignServerInit)]
public sealed record ShardServerInitAwaitingCampaignServerInitPacket : Packet;
