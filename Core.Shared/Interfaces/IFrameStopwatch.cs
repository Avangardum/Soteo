namespace Soteo.Core.Gameplay.Interfaces;

public interface IFrameStopwatch
{
    double ElapsedSinceProcess { get; }
    double ElapsedSincePhysicsProcess { get; }
}
