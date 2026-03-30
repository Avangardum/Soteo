using System.Collections.Concurrent;
using Soteo.MasterServer.Interfaces;

namespace Soteo.MasterServer;

public class Dispatcher : IDispatcher
{
    private class DispatcherSynchronizationContext(Dispatcher dispatcher) : SynchronizationContext
    {
        public override void Post(SendOrPostCallback callback, object? state) => dispatcher.Post(callback, state);
    }
    
    private readonly ConcurrentQueue<Action> _asyncContinuationQueue = [];
    private readonly ConcurrentQueue<Action> _systemTickQueue = [];
    private readonly ConcurrentQueue<Action> _servicePacketQueue = [];
    private readonly ConcurrentQueue<Action> _playerPacketQueue = [];
    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly ILogger<Dispatcher> _logger;
    private readonly Thread _mainThread;
    private volatile bool _isShuttingDown;
    private volatile int _asyncInvocationsInProgress;

    public Dispatcher(ILogger<Dispatcher> logger)
    {
        _logger = logger;
        _mainThread = new Thread(Pump);
        _mainThread.Start();
    }
    
    private void Pump()
    {
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(this));
        while (true)
        {
            _semaphore.Wait();
            bool wasShuttingDownAtCycleStart = _isShuttingDown;
            if (_asyncContinuationQueue.TryDequeue(out var asyncContinuation)) asyncContinuation();
            else if (_systemTickQueue.TryDequeue(out var systemTick)) systemTick();
            else if (_servicePacketQueue.TryDequeue(out var servicePacket)) servicePacket();
            else if (_playerPacketQueue.TryDequeue(out var playerPacket)) playerPacket();
            else if (wasShuttingDownAtCycleStart && _asyncInvocationsInProgress == 0) break;
        }
    }
    
    public void ShutDown()
    {
        _isShuttingDown = true;
        _semaphore.Release();
    }
    
    public Task<T> InvokeSync<T>(Func<T> action, DispatcherPriority priority)
    {
        if (Thread.CurrentThread == _mainThread) return Task.FromResult(action());
        ConcurrentQueue<Action> queue = priority switch
        {
            DispatcherPriority.SystemTick => _systemTickQueue,
            DispatcherPriority.ServicePacket => _servicePacketQueue,
            DispatcherPriority.PlayerPacket => _playerPacketQueue,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null)
        };
        var taskCompletionSource = new TaskCompletionSource<T>();
        queue.Enqueue(() =>
        {
            try
            {
                T result = action();
                taskCompletionSource.SetResult(result);
            }
            catch (Exception e)
            {
                taskCompletionSource.SetException(e);
            }
        });
        _semaphore.Release();
        return taskCompletionSource.Task;
    }
    
    public Task InvokeSync(Action action, DispatcherPriority priority) =>
        InvokeSync<object?>(() => { action(); return null; }, priority);

    public async Task<T> InvokeAsync<T>(Func<Task<T>> action, DispatcherPriority priority)
    {
        Interlocked.Increment(ref _asyncInvocationsInProgress);
        try
        {
            return await await InvokeSync(action, priority);
        }
        finally
        {
            Interlocked.Decrement(ref _asyncInvocationsInProgress);
        }
    }

    public async Task InvokeAsync(Func<Task> action, DispatcherPriority priority) =>
        await InvokeAsync<object?>(async () => { await action(); return null; }, priority);

    private void Post(SendOrPostCallback callback, object? state)
    {
        _asyncContinuationQueue.Enqueue(() =>
        {
            try
            {
                callback(state);
            }
            catch (Exception e)
            {
                var message = "Dispatcher.Post callback threw an exception. This is not allowed.";
                _logger.LogCritical(e, message);
                Environment.FailFast(message, e);
            }
        });
        _semaphore.Release();
    }
}