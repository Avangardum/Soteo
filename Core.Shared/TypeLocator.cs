using System.Collections.Immutable;
using System.Reflection;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.PacketSerializers;
using Soteo.Util;
using Soteo.Util.Extensions;

namespace Soteo.CampaignServer;

public static class TypeLocator
{
    private static readonly LateInit<ImmutableList<Type>> LateInitTypes = new();
    
    private static readonly LateInit<ImmutableDictionary<PacketType, IPacketSerializer>> LateInitPacketSerializers =
        new();
    
    private static readonly LateInit<ImmutableDictionary<PacketType, Type>> LateInitPacketHandlerTypes = new();

    // todo unify packet handler interface
    public static void Init(Type packetHandlerGenericDefinition, Type routingPacketHandlerType, Type iPacketHandlerType, params IReadOnlyList<Assembly> assemblies)
    {
        LateInitTypes.Value = assemblies.SelectMany(it => it.ExportedTypes).ToImmutableList();
        
        LateInitPacketSerializers.Value = Types
            .Where
            (
                it => 
                    !it.IsAbstract &&
                    it != typeof(RoutingPacketSerializer) &&
                    it.IsAssignableTo(typeof(IPacketSerializer))
            )
            .ToImmutableDictionary
            (
                it => it.GetPacketType(typeof(PacketSerializer<>)),
                it => (IPacketSerializer)Activator.CreateInstance(it)
            );
        
        LateInitPacketHandlerTypes.Value = Types
            .Where
            (
                it =>
                    !it.IsAbstract &&
                    it != routingPacketHandlerType &&
                    it.IsAssignableTo(iPacketHandlerType)
            )
            .ToImmutableDictionary(it => it.GetPacketType(packetHandlerGenericDefinition));
    }

    public static ImmutableList<Type> Types => LateInitTypes;
    
    public static ImmutableDictionary<PacketType, IPacketSerializer> PacketSerializers => LateInitPacketSerializers;
    
    public static ImmutableDictionary<PacketType, Type> PacketHandlerTypes => LateInitPacketHandlerTypes;

    public static ImmutableList<T> InstanceAllSubclasses<T>()
    {
        return Types
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(T)))
            .OrderBy(it => it.FullName)
            .Select(it => (T)Activator.CreateInstance(it))
            .ToImmutableList();
    }
}
