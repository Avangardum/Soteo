using Soteo.Core.Interfaces;
using Soteo.Util;

namespace Soteo.TestUtil;

public sealed class FakeInitializationRepo : IInitializationRepository
{
    private TaskCompletionSource _waitForInitTcs = new();
    
    public required bool IsInitialized
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
