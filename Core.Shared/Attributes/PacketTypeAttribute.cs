using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketTypeAttribute(PacketType type) : Attribute
{
    public PacketType Type => type;
}