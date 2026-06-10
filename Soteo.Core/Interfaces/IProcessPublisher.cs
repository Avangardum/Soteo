using Soteo.Core.Enums;

namespace Soteo.Core.Interfaces;

public interface IProcessPublisher
{
    IDisposable SubscribeToProcess(Action<double> handler, ProcessPriorityEnum priority, bool callWhenPaused);

    IDisposable SubscribeToProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused);

    IDisposable SubscribeToPhysicsProcess(Action<double> handler, ProcessPriorityEnum priority, bool callWhenPaused);

    IDisposable SubscribeToPhysicsProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused);
}
