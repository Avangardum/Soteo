using Soteo.Core.Interfaces;

namespace Soteo.Main.Shared.Nodes.Autoloads;

public sealed class PauseRepository : Node, IPauseRepository
{
    public bool Paused
    {
        get => GetTree().Paused;
        set => GetTree().Paused = value;
    }
}
