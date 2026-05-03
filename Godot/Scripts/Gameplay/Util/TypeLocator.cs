using System.Collections.Immutable;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.PacketHandlers;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Util;

public static class TypeLocator
{
    public static readonly Dictionary<PacketType, Type> PacketHandlerTypes;
    
    static TypeLocator()
    {
        PacketHandlerTypes = typeof(IPacketHandler).Assembly.ExportedTypes
            .Where(it =>
                !it.IsAbstract && it != typeof(RoutingPacketHandler) && it.IsAssignableTo(typeof(IPacketHandler)))
            .ToDictionary(it => it.GetPacketType(typeof(PacketHandler<>)));
    }
    
    public static ImmutableList<T> InstanceAllSubclasses<T>()
    {
        return typeof(T).Assembly.DefinedTypes
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(T)))
            .OrderBy(it => it.FullName)
            .Select(it => (T)Activator.CreateInstance(it))
            .ToImmutableList();
    }
}