using System.ComponentModel.DataAnnotations;

namespace Soteo.Core;

public sealed record ShardOptions
{
    [Required]
    public required Guid ShardId { get; init; }
}
