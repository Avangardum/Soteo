using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Messages.Master;

[MessageType(MessageType.SpawnCharacter)]
public sealed record SpawnCharacterMessage : RelayedMessage
{
    public Guid SpawnPointId { get; set; }
}