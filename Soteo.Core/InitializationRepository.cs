namespace Soteo.Core;

public sealed class InitializationRepository : IInitializationRepository
{
    private TaskCompletionSource _waitForInitTcs = new();

    public bool IsInitialized
    {
        get;
        set
        {
            if (value == field) return;
            field = value;
            if (value)
            {
                TaskCompletionSource oldTcs = _waitForInitTcs;
                _waitForInitTcs = new TaskCompletionSource();
                oldTcs.SetResult();
            }
        }
    }

    public async Task WaitForInitAsync()
    {
        if (!IsInitialized)
            await _waitForInitTcs.Task;
    }
}
