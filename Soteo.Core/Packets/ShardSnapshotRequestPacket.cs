using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshotRequest)]
public sealed record ShardSnapshotRequestPacket : Packet;

