using System.Numerics;

namespace Soteo.Core.Commands;

public record MoveCommand(Vector2 Position) : ICommand;
