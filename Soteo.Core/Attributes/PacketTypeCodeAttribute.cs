using Soteo.Core.Enums;

namespace Soteo.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class PacketTypeCodeAttribute(PacketTypeCode typeCode) : Attribute
{
    public PacketTypeCode TypeCode => typeCode;
}
