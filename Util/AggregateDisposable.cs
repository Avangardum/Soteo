namespace Soteo.Shared;

public sealed class AggregateDisposable(params IEnumerable<IDisposable> inner) : IDisposable
{
    private bool _isDisposed;
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        foreach (IDisposable disposable in inner)
            disposable.Dispose();
    }
}