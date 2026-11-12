namespace Soteo.Core;

public enum ProcessPriorityEnum
{
    Min = -1000,
    FrameStopwatch,
    Communicator,
    Default = 0,
    OverheadUi,
    SynchronizationServer,
    CurrentTickRepository
}
