using Soteo.Client.Interfaces;

namespace Soteo.Client.Commands;

public record MoveCommand(Vector2 Position) : ICommand;