using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

/// <summary>
/// WebRTC ICE candidate info in the Godot format.
/// </summary>
[PacketType(PacketType.WebrtcIceCandidate)]
public sealed record WebrtcIceCandidatePacket : RelayedPacket
{
    public string Media { get; set; } = "";
    public int Index { get; set; }
    public string Name { get; set; } = "";
}