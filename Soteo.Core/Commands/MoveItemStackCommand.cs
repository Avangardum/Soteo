using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

public sealed record MoveItemStackCommand : ICommand
{
    public required int FromIndex { get; init; }
    public required int ToIndex { get; init; }
}
