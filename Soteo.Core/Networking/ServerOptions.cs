using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Dto.Options;

public sealed record ServerOptions
{
    [Required]
    public required string ServerSecret { get; init; }
}
