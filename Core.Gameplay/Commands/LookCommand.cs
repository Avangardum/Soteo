using System.Numerics;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Commands;

public record LookCommand(Vector2 Position) : ICommand;