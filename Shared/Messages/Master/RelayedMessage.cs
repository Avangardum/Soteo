using Soteo.Shared.Messages.Shared;

namespace Soteo.Shared.Messages.Master;

public abstract record RelayedMessage : Message
{
    public Guid PeerId { get; set; }
}