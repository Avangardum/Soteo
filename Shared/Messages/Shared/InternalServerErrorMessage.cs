using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Messages.Shared;

[MessageType(MessageType.InternalServerError)]
public sealed record InternalServerErrorMessage : Message;