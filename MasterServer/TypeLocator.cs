using System.Collections.Immutable;
using Soteo.MasterServer.Interfaces;
using Soteo.MasterServer.MessageHandlers;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.MasterServer;

public static class TypeLocator
{
    
    public static readonly ImmutableDictionary<MessageType, Type> MessageHandlerTypes;
    
    static TypeLocator()
    {
        MessageHandlerTypes = typeof(IMessageHandler).Assembly.ExportedTypes
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(IMessageHandler)))
            .ToImmutableDictionary(it => it.GetMessageType(typeof(MessageHandler<>)));
    }
}