using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Commands;

public record MoveCommand(GdVector2 Position) : ICommand;