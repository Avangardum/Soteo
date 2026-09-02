using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Options;

public sealed record SideOptions
{
    public required Side Side { get; init; }
}
