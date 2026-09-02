using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Dto.Options;

public sealed record ShardOptions
{
    [Required]
    public required Guid ShardId { get; init; }
}
