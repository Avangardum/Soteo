using System.Collections.Generic;
using System.Linq;
using Soteo.Client.MessageHandlers;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Client;

public static class TypeLocator
{
    public static readonly Dictionary<MessageType, Type> MessageHandlerTypes;
    
    static TypeLocator()
    {
        MessageHandlerTypes = typeof(IMessageHandler).Assembly.ExportedTypes
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(IMessageHandler)))
            .ToDictionary(it => it.GetMessageType(typeof(MessageHandler<>)));
    }
}