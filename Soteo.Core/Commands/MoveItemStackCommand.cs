namespace Soteo.Core.Commands;

public sealed record MoveItemStackCommand : Command
{
    public required int FromIndex { get; init; }
    public required int ToIndex { get; init; }
}
