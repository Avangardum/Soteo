using System.Collections.Concurrent;
using System.Collections.Generic;
using Soteo.Shared.Attributes;

namespace Soteo.Client;

public sealed class AsyncExceptionCollector : Node
{
    private static readonly ConcurrentQueue<Exception> Exceptions = [];

    public override void _Process(float delta)
    {
        if (Exceptions.TryDequeue(out var e))
            throw e;
    }
    
    public static void Collect(Exception e) => Exceptions.Enqueue(e);
}