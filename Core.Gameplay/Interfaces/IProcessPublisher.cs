using Soteo.Core.Gameplay.Enums;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IProcessPublisher
{
    IDisposable SubscribeToProcess(Action<double> subscription, ProcessPriorityEnum priority);

    IDisposable SubscribeToProcess(Action subscription, ProcessPriorityEnum priority);

    IDisposable SubscribeToPhysicsProcess(Action<double> subscription, ProcessPriorityEnum priority);

    IDisposable SubscribeToPhysicsProcess(Action subscription, ProcessPriorityEnum priority);
}
