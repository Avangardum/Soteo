using Soteo.Core.Interfaces;

namespace Soteo.Main.Shared.Nodes.Autoloads;

// TODO change into repository for campaign global state with authoritative copy in the campaign servers and
// synchronized copies in gameplay
public sealed class PauseRepository : Node, IPauseRepository
{
    public bool IsPaused
    {
        get => GetTree().Paused;
        set => GetTree().Paused = value;
    }
}
