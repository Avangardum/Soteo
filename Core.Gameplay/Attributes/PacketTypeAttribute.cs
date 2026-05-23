using Soteo.Shared.Enums;

namespace Soteo.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketTypeAttribute(PacketType type) : Attribute
{
    public PacketType Type => type;
}