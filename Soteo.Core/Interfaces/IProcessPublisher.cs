using Soteo.Core.Enums;

namespace Soteo.Core.Interfaces;

/// <summary>
/// Allows to subscribe to Godot process and physics process events.
/// To simplify testing of classes dependent on this make the handler public and call it directly from tests,
/// while supplying a no-op process publisher stub.
/// </summary>
public interface IProcessPublisher
{
    IDisposable SubscribeToProcess(Action<double> handler, ProcessPriorityEnum priority, bool callWhenPaused);

    IDisposable SubscribeToPhysicsProcess(Action<double> handler, ProcessPriorityEnum priority, bool callWhenPaused);
}
