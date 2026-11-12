namespace Soteo.Core;

public interface IFrameStopwatch
{
    double ElapsedSinceProcess { get; }
    double ElapsedSincePhysicsProcess { get; }
}
