namespace Soteo.Util;

public sealed class TaskCompletionSource
{
    private readonly TaskCompletionSource<Never?> _inner = new();
    
    public Task Task => _inner.Task;
    
    public void SetResult() => _inner.SetResult(null);
    
    public void SetException(Exception e) => _inner.SetException(e);
    
    public void SetException(IEnumerable<Exception> e) => _inner.SetException(e);
    
    public void SetCanceled() => _inner.SetCanceled();
    
    public bool TrySetResult() => _inner.TrySetResult(null);
    
    public bool TrySetException(Exception e) => _inner.TrySetException(e);
    
    public bool TrySetException(IEnumerable<Exception> e) => _inner.TrySetException(e);
    
    public bool TrySetCanceled() => _inner.TrySetCanceled();
}
