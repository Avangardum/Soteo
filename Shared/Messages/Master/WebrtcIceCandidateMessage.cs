using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Messages.Master;

/// <summary>
/// WebRTC ICE candidate info in the Godot format.
/// </summary>
[MessageType(MessageType.WebrtcIceCandidate)]
public sealed record WebrtcIceCandidateMessage : RelayedMessage
{
    public string Media { get; set; } = "";
    public int Index { get; set; }
    public string Name { get; set; } = "";
}