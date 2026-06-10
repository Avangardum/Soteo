using System.Collections.Immutable;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Extensions;
using Soteo.Core.Shared.Interfaces;

namespace Soteo.Core.Shared;

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
            .ToImmutableDictionary(it => it.GetPacketType(typeof(PacketHandler<>)));
    }
}
