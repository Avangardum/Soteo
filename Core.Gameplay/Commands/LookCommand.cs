using System.Numerics;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Commands;

public record LookCommand(Vector2 Position) : ICommand;