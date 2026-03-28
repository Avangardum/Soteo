using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Messages.Master;

[MessageType(MessageType.InvalidMessage)]
public sealed record InvalidMessageMessage : Message;