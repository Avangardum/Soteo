using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketTypeAttribute(PacketType type) : Attribute // todo rename
{
    public PacketType Type => type;
}