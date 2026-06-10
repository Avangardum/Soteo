using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketTypeCodeAttribute(PacketTypeCode typeCode) : Attribute
{
    public PacketTypeCode TypeCode => typeCode;
}
