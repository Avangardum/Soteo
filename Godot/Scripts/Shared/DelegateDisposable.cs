namespace Soteo.Shared;

public sealed class DelegateDisposable(Action dispose) : IDisposable
{
    private bool _isDisposed;
    
    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        dispose();
    }
}
