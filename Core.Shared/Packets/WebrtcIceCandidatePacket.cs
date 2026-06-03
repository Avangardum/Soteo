using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

/// <summary>
/// WebRTC ICE candidate info in the Godot format.
/// </summary>
[PacketTypeCode(PacketTypeCode.WebrtcIceCandidate)]
public sealed record WebrtcIceCandidatePacket : RelayedPacket
{
    public required string Media { get; init; }
    public required int Index { get; init; }
    public required string Name { get; init; }
}
