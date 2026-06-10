using System.Numerics;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

public record LookCommand(Vector2 Position) : ICommand;