using Microsoft.Extensions.Logging;

namespace Soteo.Util;

/// <summary>
/// The standard console logger doesn't work in a browser, so this one is used instead
/// </summary>
public sealed class SimpleConsoleLogger(string categoryName) : ILogger
{
    public void Log<TState>
    (
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        Console.WriteLine($"{logLevel} {categoryName}");
        Console.WriteLine($"    {formatter(state, exception)}");
        Console.WriteLine();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public sealed class Provider : ILoggerProvider
    {
        public void Dispose() { }
        public ILogger CreateLogger(string categoryName) => new SimpleConsoleLogger(categoryName);
    }
}
