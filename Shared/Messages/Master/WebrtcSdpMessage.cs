using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Messages.Master;

[MessageType(MessageType.WebrtcSdp)]
public sealed record WebrtcSdpMessage : RelayedMessage
{
    public string SdpType { get; set; } = "";
    public string Sdp { get; set; } = "";
}