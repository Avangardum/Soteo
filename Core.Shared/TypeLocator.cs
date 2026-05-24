using System.Collections.Immutable;
using System.Reflection;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Extensions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.PacketSerializers;
using Soteo.Util;
using Soteo.Util.Extensions;

namespace Soteo.Core.Shared;

public static class TypeLocator
{
    private static readonly LateInit<ImmutableList<Type>> LateInitTypes = new();
    
    private static readonly LateInit<ImmutableDictionary<PacketType, IPacketSerializer>> LateInitPacketSerializers =
        new();
    
    private static readonly LateInit<ImmutableDictionary<PacketType, Type>> LateInitPacketHandlerTypes = new();

    public static void Init(params IReadOnlyList<Assembly> assemblies)
    {
        LateInitTypes.Value = assemblies.SelectMany(it => it.ExportedTypes).ToImmutableList();
        
        LateInitPacketSerializers.Value = Types
            .Where
            (
                it => 
                    !it.IsAbstract &&
                    it.BaseType is { IsGenericType: true } &&
                    it.IsAssignableTo(typeof(IPacketSerializer))
            )
            .ToImmutableDictionary<Type, PacketType, IPacketSerializer>(
                it => it.GetPacketType(typeof(PacketSerializer<>)),
                it => (IPacketSerializer)Activator.CreateInstance(it)
            );
        
        LateInitPacketHandlerTypes.Value = Types
            .Where
            (
                it =>
                    !it.IsAbstract &&
                    it.BaseType is { IsGenericType: true } &&
                    it.IsAssignableTo(typeof(IPacketHandler))
            )
            .ToImmutableDictionary(it => it.GetPacketType(typeof(PacketHandler<>)));
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
