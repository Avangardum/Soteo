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

    public IDisposable SubscribeToProcess(Action<double> handler, ProcessPriorityEnum priority, bool callWhenPaused)
    {
        return Subscribe(it => it.Process, handler, priority, callWhenPaused);
    }

    public IDisposable SubscribeToProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused)
    {
        return SubscribeToProcess(_ => handler(), priority, callWhenPaused);
    }

    public IDisposable SubscribeToPhysicsProcess
    (
        Action<double> handler,
        ProcessPriorityEnum priority,
        bool callWhenPaused
    )
    {
        return Subscribe(it => it.PhysicsProcess, handler, priority, callWhenPaused);
    }

    public IDisposable SubscribeToPhysicsProcess(Action handler, ProcessPriorityEnum priority, bool callWhenPaused)
    {
        return SubscribeToPhysicsProcess(_ => handler(), priority, callWhenPaused);
    }

    private IDisposable Subscribe
    (
        Func<Subscriptions, List<Action<double>>> listSelector,
        Action<double> handler,
        ProcessPriorityEnum priority,
        bool callWhenPaused
    )
    {
        Action<double> processedHandler;
        if (callWhenPaused)
        {
            processedHandler = handler;
        }
        else
        {
            processedHandler = it =>
            {
                if (!GetTree().Paused)
                    handler(it);
            };
        }
        
        if (!_subscriptionsByPriority.ContainsKey(priority))
        {
            var subscriptions = new Subscriptions([], []);
            _subscriptionsByPriority[priority] = subscriptions;
            var processListener = new ProcessListener(subscriptions, priority);
            AddChild(processListener);
        }
        
        List<Action<double>> list = listSelector(_subscriptionsByPriority[priority]);
        list.Add(processedHandler);
        return new DelegateDisposable(() => list.Remove(processedHandler));
    }
    
    private sealed record Subscriptions(List<Action<double>> Process, List<Action<double>> PhysicsProcess);
    
    private sealed class ProcessListener(Subscriptions subscriptions, ProcessPriorityEnum priority) : Node
    {
        public override void _Ready()
        {
            Name = nameof(ProcessListener) + " " + priority;
            ProcessPriority = (int)priority;
            PauseMode = PauseModeEnum.Process;
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
