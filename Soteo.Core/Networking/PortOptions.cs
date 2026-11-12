using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Networking;

public sealed record PortOptions
{
    [Range(1024, 49151)]
    public required int Port { get; init; }
}
