using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Commands;

public record MoveCommand(Vector2 Position) : ICommand;