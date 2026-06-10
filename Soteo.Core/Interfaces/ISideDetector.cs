namespace Soteo.Core.Gameplay.Interfaces;

public interface ISideDetector
{
    bool IsServer { get; }
    bool IsClient { get; }
}
