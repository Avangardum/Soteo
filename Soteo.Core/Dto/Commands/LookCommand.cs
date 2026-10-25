using System.Numerics;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Dto.Commands;

public record LookCommand(Vector2 Position) : ICommand;