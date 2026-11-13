namespace Soteo.Core;

public enum ProcessPriority
{
    Min = -1000,
    FrameStopwatch,
    Communicator,
    Default = 0,
    OverheadUi,
    SynchronizationServer,
    CurrentTickRepository
}
