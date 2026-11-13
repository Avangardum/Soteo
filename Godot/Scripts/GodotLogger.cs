using Microsoft.Extensions.Logging;

namespace Soteo.Main.Shared;

internal sealed class GodotLogger : ILogger
{
    private readonly string _categoryName;

    public GodotLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public void Log<TState>
    (
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        Action<string> print = logLevel is LogLevel.Critical or LogLevel.Error ? GD.PrintErr : GD.Print;
        print($"{logLevel} {_categoryName}\n    {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public sealed class Provider : ILoggerProvider
    {
        public void Dispose() { }

        public ILogger CreateLogger(string categoryName) => new GodotLogger(categoryName);
    }
}
