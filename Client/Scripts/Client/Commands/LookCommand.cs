using Soteo.Client.Interfaces;

namespace Soteo.Client.Commands;

public record LookCommand(Vector2 Position) : ICommand;