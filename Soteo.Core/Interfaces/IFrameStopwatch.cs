namespace Soteo.Core.Interfaces;

public interface IFrameStopwatch
{
    double ElapsedSinceProcess { get; }
    double ElapsedSincePhysicsProcess { get; }
}
