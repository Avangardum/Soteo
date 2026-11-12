using Soteo.Core.Interfaces;
using Soteo.Util;

namespace Soteo.Core.Services.Repositories;

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
