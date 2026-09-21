namespace Soteo.Util;

public sealed class DisposableTimeout : IDisposable
{
    private Action _onTimeout;
    private bool _isDisposed;

    public DisposableTimeout(TimeProvider timeProvider, TimeSpan timeout, Action onTimeout)
    {
        _onTimeout = onTimeout;
        timeProvider.Delay(timeout).ContinueWithinContext(OnTimeout).CollectException();
    }

    public void Dispose() => _isDisposed = true;

    private void OnTimeout()
    {
        if (_isDisposed) return;
        _onTimeout();
    }
}
