namespace Soteo.Core.PacketHandlers;

public static class PacketHandlerLocator<TAttribute> where TAttribute : Attribute
{
    private static IReadOnlyDictionary<Type, Type>? _typesByPacketType;

    public static Type? TypeFor(Type packetType, ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.GetOrDefault(packetType);
    }

    public static IReadOnlyList<Type> AllTypes(ITypeLocator typeLocator)
    {
        _typesByPacketType ??= InitTypesByPacketType(typeLocator);
        return _typesByPacketType.Values.ToList().AsReadOnly();
    }

    private static IReadOnlyDictionary<Type, Type> InitTypesByPacketType(ITypeLocator typeLocator)
    {
        return typeLocator
            .ConcreteSubclassesOf<IPacketHandler>(where: it => it.HasAttribute<TAttribute>())
            .ToDictionary<Type, Type>
            (
                it => it.SingleTypeArgOfGenericDefinitionOrNull(typeof(PacketHandler<>)).Required
            ).AsReadOnly();
    }
}
