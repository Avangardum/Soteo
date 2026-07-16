namespace Soteo.Main.Shared;

/// <summary>
/// TimeProvider implementation that's safe to use in Godot. Use it instead of Task.Delay,
/// as it causes issues in the browser build.
/// </summary>
public sealed class GodotTimeProvider(SceneTree sceneTree) : TimeProvider
{
    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new Timer();
        
        if (dueTime == TimeSpan.Zero)
        {
            OnTimeout(callback, state, period, timer);
        }
        else if (dueTime == Timeout.InfiniteTimeSpan)
        {
            
        }
        else
        {
            SceneTreeTimer sceneTreeTimer = sceneTree.CreateTimer((float)dueTime.TotalSeconds);
            sceneTreeTimer.Connect("timeout", () => OnTimeout(callback, state, period, timer));
        }
        
        return timer;
    }
    
    private void OnTimeout(TimerCallback callback, object? state, TimeSpan period, Timer timer)
    {
        if (timer.IsDisposed) return;
        callback(state);
        if (period == TimeSpan.Zero || period == Timeout.InfiniteTimeSpan) return;
        SceneTreeTimer sceneTreeTimer = sceneTree.CreateTimer((float)period.TotalSeconds);
        sceneTreeTimer.Connect("timeout", () => OnTimeout(callback, state, period, timer));
    }
    
    private class Timer : ITimer
    {
        public bool IsDisposed { get; private set; }
        
        public void Dispose() => IsDisposed = true;

        public async ValueTask DisposeAsync() => Dispose();

        public bool Change(TimeSpan dueTime, TimeSpan period) => throw new NotSupportedException();
    }
}
