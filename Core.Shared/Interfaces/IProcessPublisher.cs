using Soteo.Core.Gameplay.Enums;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IProcessPublisher
{
    IDisposable SubscribeToProcess(Action<double> handler, ProcessPriorityEnum priority, bool callWhenPaused);

    IDisposable SubscribeToProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused);

    IDisposable SubscribeToPhysicsProcess(Action<double> handler, ProcessPriorityEnum priority, bool callWhenPaused);

    IDisposable SubscribeToPhysicsProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused);
}
