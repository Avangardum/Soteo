using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Messages.PlayerShard;

[MessageType(MessageType.Move)]
public sealed record MoveMessage : Message
{
    
}