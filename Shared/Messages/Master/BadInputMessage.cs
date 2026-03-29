using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Messages.Master;

[MessageType(MessageType.BadInput)]
public sealed record BadInputMessage : Message
{
    public string Reason { get; set; } = "";
}