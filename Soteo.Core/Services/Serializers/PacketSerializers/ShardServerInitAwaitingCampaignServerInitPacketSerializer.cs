using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class ShardServerInitAwaitingCampaignServerInitPacketSerializer(ISerializationHelper s) :
    EmptyPacketSerializer<ShardServerInitAwaitingCampaignServerInitPacket>(s);
