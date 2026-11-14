using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Soteo.Core;
using Soteo.Core.Serialization;

BenchmarkRunner.Run<Benchmarks>();

public class Benchmarks
{
    [Benchmark]
    public object? Benchmark()
    {
        return null;
    }
}
