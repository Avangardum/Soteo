using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Shared.Messages.Shared;

public abstract record Message
{
    protected Message()
    {
        Type = GetType().GetRequiredAttribute<MessageTypeAttribute>().Type;
        CorrelationId = Guid.NewGuid();
    }
    
    public MessageType Type { get; }
    public Guid CorrelationId { get; set; }
}