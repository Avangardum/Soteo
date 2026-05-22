using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace Soteo.Shared.Nodes.Autoloads;

public static class AsyncExceptionCollector
{
    private static readonly Queue<Exception> Exceptions = [];

    public static void Process()
    {
        if (Exceptions.Count > 0)
            ExceptionDispatchInfo.Capture(Exceptions.Dequeue()).Throw();
    }
    
    public static void Collect(Exception e) => Exceptions.Enqueue(e);
}