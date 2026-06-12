using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

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
