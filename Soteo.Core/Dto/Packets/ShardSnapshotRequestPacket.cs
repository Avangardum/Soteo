using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshotRequest)]
public sealed record ShardSnapshotRequestPacket : Packet;

