using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Messages.Master;

[MessageType(MessageType.Handshake)]
public sealed record HandshakeMessage : Message
{
    public string Token { get; set; } = "";
    public string Version { get; set; } = "";
}