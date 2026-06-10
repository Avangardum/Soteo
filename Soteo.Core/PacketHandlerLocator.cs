using System.Collections.Immutable;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core;

public static class PacketHandlerLocator<TAttribute> where TAttribute : Attribute
{
    private static ImmutableDictionary<PacketTypeCode, Type>? _typesByPacketType;
    
    public static Type? TypeFor(PacketTypeCode packetTypeCode, ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.GetOrDefault(packetTypeCode);
    }
    
    public static IReadOnlyList<Type> AllTypes(ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.Values.ToImmutableList();
    }
    
    private static ImmutableDictionary<PacketTypeCode, Type> InitTypesByPacketType(ITypeLocator typeLocator)
    {
        return typeLocator
            .ConcreteSubclassesOf<IPacketHandler>(where: it => it.HasAttribute<TAttribute>())
            .ToImmutableDictionary<Type, PacketTypeCode>(it => it.GetPacketType(typeof(PacketHandler<>)));
    }
}
