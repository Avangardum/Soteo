namespace Soteo.Util;

public static class Wait
{
    public static async Task ForEventAsync(Action<Action> add, Action<Action> remove)
    {
        var tcs = new TaskCompletionSource();
        Action onEvent = () => tcs.TrySetResult();
        add(onEvent);
        await tcs.Task;
        remove(onEvent);
    }
}
