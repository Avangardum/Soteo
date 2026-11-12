using System.Numerics;

namespace Soteo.Core.Commands;

public record LookCommand(Vector2 Position) : ICommand;
