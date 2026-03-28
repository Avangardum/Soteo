using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Messages.Master;

[MessageType(MessageType.Ok)]
public sealed record OkMessage : Message;