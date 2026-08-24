using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

/// <summary>
/// Sent from the campaign server to shard servers to notify that it has finished initializing and
/// that they should finish initializing as well
/// </summary>
[PacketTypeCode(PacketTypeCode.CampaignInitialized)]
public sealed record CampaignInitializedPacket : Packet;
