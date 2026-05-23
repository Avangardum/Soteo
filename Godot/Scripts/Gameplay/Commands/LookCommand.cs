using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Commands;

public record LookCommand(GdVector2 Position) : ICommand;