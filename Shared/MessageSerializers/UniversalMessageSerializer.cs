using Soteo.Shared.Enums;
using Soteo.Shared.Exceptions;
using Soteo.Shared.Extensions;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.MessageSerializers;

public sealed class UniversalMessageSerializer : IMessageSerializer
{
    private readonly Dictionary<MessageType, IMessageSerializer> _serializers;

    public UniversalMessageSerializer()
    {
        _serializers = typeof(IMessageSerializer).Assembly.ExportedTypes
            .Where(it =>
                !it.IsAbstract &&
                it != typeof(UniversalMessageSerializer) &&
                it.IsAssignableTo(typeof(IMessageSerializer)))
            .ToDictionary
            (
                it => it.GetMessageType(typeof(MessageSerializer<>)),
                it => (IMessageSerializer)Activator.CreateInstance(it)!
            );
    }
    
    public Message Deserialize(Span<byte> bytes)
    {
        var type = (MessageType)bytes[0];
        if (!_serializers.TryGetValue(type, out IMessageSerializer serializer))
            throw new BadMessageException("Invalid message type");
        return serializer.Deserialize(bytes);
    }

    public byte[] Serialize(Message message)
    {
        if (!_serializers.TryGetValue(message.Type, out IMessageSerializer serializer))
            throw new InvalidOperationException($"Missing serializer for message type {message.Type}");
        return serializer.Serialize(message);
    }
}