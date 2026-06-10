namespace Soteo.Core.Interfaces;

public interface ISideDetector
{
    bool IsServer { get; }
    bool IsClient { get; }
}
