using Soteo.Shared.Enums;

namespace Soteo.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class MessageTypeAttribute(MessageType type) : Attribute
{
    public MessageType Type => type;
}