using System.ComponentModel.DataAnnotations;

namespace Soteo.Core.Dto.Options;

public sealed record IntercomOptions
{
    [Required]
    public required string IntercomSecret { get; init; }
}
