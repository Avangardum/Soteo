using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Shared.Nodes;

public sealed class ProcessPublisher : Node, IProcessPublisher
{
    private readonly Dictionary<ProcessPriorityEnum, Subscriptions> _subscriptionsByPriority = [];

    public override void _Ready()
    {
        Name = nameof(ProcessPublisher);
    }

    public IDisposable SubscribeToProcess(Action<double> subscription, ProcessPriorityEnum priority) =>
        Subscribe(it => it.Process, subscription, priority);

    public IDisposable SubscribeToProcess(Action subscription, ProcessPriorityEnum priority) =>
        SubscribeToProcess(_ => subscription(), priority);

    public IDisposable SubscribeToPhysicsProcess(Action<double> subscription, ProcessPriorityEnum priority) =>
        Subscribe(it => it.PhysicsProcess, subscription, priority);

    public IDisposable SubscribeToPhysicsProcess(Action subscription, ProcessPriorityEnum priority) =>
        SubscribeToPhysicsProcess(_ => subscription(), priority);

    private IDisposable Subscribe
    (
        Func<Subscriptions, List<Action<double>>> listSelector,
        Action<double> subscription,
        ProcessPriorityEnum priority
    )
    {
        if (!_subscriptionsByPriority.ContainsKey(priority))
        {
            var subscriptions = new Subscriptions([], []);
            _subscriptionsByPriority[priority] = subscriptions;
            var processListener = new ProcessListener(subscriptions, priority);
            AddChild(processListener);
        }
        
        List<Action<double>> list = listSelector(_subscriptionsByPriority[priority]);
        list.Add(subscription);
        return new DelegateDisposable(() => list.Remove(subscription));
    }
    
    private sealed record Subscriptions(List<Action<double>> Process, List<Action<double>> PhysicsProcess);
    
    private sealed class ProcessListener(Subscriptions subscriptions, ProcessPriorityEnum priority) : Node
    {
        public override void _Ready()
        {
            ProcessPriority = (int)priority;
            Name = nameof(ProcessListener) + " " + priority;
        }

        public override void _Process(float delta)
        {
            foreach (Action<double> sub in subscriptions.Process.ToList())
                sub(delta);
        }
        
        public override void _PhysicsProcess(float delta)
        {
            foreach (Action<double> sub in subscriptions.PhysicsProcess.ToList())
                sub(delta);
        }
    }
}
