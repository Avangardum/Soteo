using System.Numerics;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

public record MoveCommand(Vector2 Position) : ICommand;