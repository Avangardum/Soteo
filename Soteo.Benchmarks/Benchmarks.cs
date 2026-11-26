using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<Benchmarks>();

public class Benchmarks
{
    [Benchmark]
    public object? Benchmark()
    {
        return null;
    }
}
