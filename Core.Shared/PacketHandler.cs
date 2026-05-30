using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Exceptions;
using Soteo.Core.Shared.Extensions;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared;

public static class PacketHandler
{
    public static readonly ImmutableDictionary<PacketTypeCode, Type> TypesByPacketType = TypeLocator
        .ConcreteSubclassesOf<IPacketHandler>(where: it => it.BaseType.Required.IsGenericType)
        .ToImmutableDictionary(it => it.GetPacketType(typeof(PacketHandler<>)));
    
    public static Type? TypeFor(PacketTypeCode packetTypeCode) => TypesByPacketType.GetOrDefault(packetTypeCode);
}

public abstract class PacketHandler<T> : IPacketHandler where T : Packet
{
    Task IPacketHandler.HandleAsync(Packet packet, Guid senderId) => HandleAsync((T)packet, senderId);
    
    public virtual Task HandleAsync(T packet, Guid senderId)
    {
        Handle(packet, senderId);
        return Task.CompletedTask;
    }
    
    protected virtual void Handle(T packet, Guid senderId) {}
    
    protected void Validate([DoesNotReturnIf(false)] bool condition, string reason)
    {
        if (!condition) throw new BadPacketException(reason);
    }
}
