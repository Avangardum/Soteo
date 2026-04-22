using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace Soteo.Shared.Nodes.Autoloads;

public sealed class AsyncExceptionCollector : Node
{
    private static readonly ConcurrentQueue<Exception> Exceptions = [];

    public override void _Process(float delta)
    {
        if (Exceptions.TryDequeue(out var e))
            ExceptionDispatchInfo.Capture(e).Throw();
    }
    
    public static void Collect(Exception e) => Exceptions.Enqueue(e);
}