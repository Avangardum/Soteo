using Soteo.Gameplay.Enums;

namespace Soteo.Shared.Interfaces;

public interface IProcessPublisher
{
    IDisposable SubscribeToProcess
    (
        Action<double> subscription,
        ProcessPriorityEnum priority = ProcessPriorityEnum.Default
    );

    IDisposable SubscribeToProcess
    (
        Action subscription,
        ProcessPriorityEnum priority = ProcessPriorityEnum.Default
    );

    IDisposable SubscribeToPhysicsProcess
    (
        Action<double> subscription,
        ProcessPriorityEnum priority = ProcessPriorityEnum.Default
    );

    IDisposable SubscribeToPhysicsProcess
    (
        Action subscription,
        ProcessPriorityEnum priority = ProcessPriorityEnum.Default
    );
}