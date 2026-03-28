using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Messages.Master;

[MessageType(MessageType.CharacterRecalled)]
public sealed record CharacterRecalledMessage : Message
{
    public Guid CharacterId { get; set; }
}