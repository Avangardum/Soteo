using System.Collections.Immutable;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Util;

namespace Soteo.TestUtil;

public sealed class FakeProcessPublisher : IProcessPublisher // todo remove
{
    private readonly ImmutableDictionary<ProcessPriorityEnum, List<Action<double>>> _processSubscriptions =
        Enum.GetValues<ProcessPriorityEnum>().ToImmutableDictionary(it => it, _ => new List<Action<double>>());
    private readonly ImmutableDictionary<ProcessPriorityEnum, List<Action<double>>> _physicsProcessSubscriptions =
        Enum.GetValues<ProcessPriorityEnum>().ToImmutableDictionary(it => it, _ => new List<Action<double>>());
    
    public bool IsPaused { get; set; }
    
    public IDisposable SubscribeToProcess
    (
        Action<double> handler,
        ProcessPriorityEnum priority,
        bool callWhenPaused
    )
    {
        return Subscribe(_processSubscriptions, handler, priority, callWhenPaused);
    }

    public IDisposable SubscribeToPhysicsProcess
    (
        Action<double> handler,
        ProcessPriorityEnum priority,
        bool callWhenPaused
    )
    {
        return Subscribe(_physicsProcessSubscriptions, handler, priority, callWhenPaused);
    }
    
    private IDisposable Subscribe
    (
        ImmutableDictionary<ProcessPriorityEnum, List<Action<double>>> subscriptions,
        Action<double> handler,
        ProcessPriorityEnum priority,
        bool callWhenPaused
    )
    {
        Action<double> processedHandler = callWhenPaused ? handler : it => { if (!IsPaused) handler(it); };
        subscriptions[priority].Add(processedHandler);
        return new DelegateDisposable(() => subscriptions[priority].Remove(processedHandler));
    }
    
    public void Process(double delta) => Process(_processSubscriptions, delta);
    
    public void PhysicsProcess(double delta) => Process(_physicsProcessSubscriptions, delta);
    
    private void Process(ImmutableDictionary<ProcessPriorityEnum, List<Action<double>>> subscriptions, double delta)
    {
        foreach (var priority in Enum.GetValues<ProcessPriorityEnum>().Order())
        {
            foreach (Action<double> subscription in subscriptions[priority])
                subscription(delta);
        }
    }
}
