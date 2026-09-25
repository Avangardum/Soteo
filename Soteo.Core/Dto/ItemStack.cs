using Soteo.Core.Items;

namespace Soteo.Core.Dto;

public sealed record ItemStack
{
    public required Item Item { get; init; }
    public required int Count { get; init; }
}
