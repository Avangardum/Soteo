using System.Runtime.ExceptionServices;

namespace Soteo.Util;

public static class AsyncExceptionCollector
{
    private static readonly Queue<Exception> Exceptions = [];

    public static void Process()
    {
        if (Exceptions.Count > 0)
        {
            // Throw while preserving an original stack trace
            ExceptionDispatchInfo.Capture(Exceptions.Dequeue()).Throw();
        }
    }
    
    public static void Collect(Exception e) => Exceptions.Enqueue(e);
}
