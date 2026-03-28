using System.Collections.Immutable;
using Soteo.MasterServer.Interfaces;
using Soteo.MasterServer.MessageHandlers;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.MessageSerializers;

namespace Soteo.MasterServer;

public static class TypeLocator
{
    public static readonly ImmutableDictionary<MessageType, IMessageSerializer> MessageSerializers;
    public static readonly ImmutableDictionary<MessageType, Type> MessageHandlerTypes;
    
    static TypeLocator()
    {
        MessageSerializers = typeof(IMessageSerializer).Assembly.ExportedTypes
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(IMessageSerializer)))
            .ToImmutableDictionary
            (
                it => GetMessageType(it, typeof(MessageSerializer<>)),
                it => (IMessageSerializer)Activator.CreateInstance(it)!
            );
        
        MessageHandlerTypes = typeof(IMessageHandler).Assembly.ExportedTypes
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(IMessageHandler)))
            .ToImmutableDictionary(it => GetMessageType(it, typeof(MessageHandler<>)));
    }
    
    private static MessageType GetMessageType(Type type, Type baseGenericClass)
    {
        return type.BaseTypes
            .Single(bt => bt.IsConstructedGenericType && bt.GetGenericTypeDefinition() == baseGenericClass)
            .GenericTypeArguments.Single()
            .GetRequiredAttribute<MessageTypeAttribute>().Type;
    }
}