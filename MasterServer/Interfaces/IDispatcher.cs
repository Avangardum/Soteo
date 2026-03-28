namespace Soteo.MasterServer.Interfaces;

public interface IDispatcher
{
    Task<T> InvokeSync<T>(Func<T> action, DispatcherPriority priority);
    Task InvokeSync(Action action, DispatcherPriority priority);
    Task<T> InvokeAsync<T>(Func<Task<T>> action, DispatcherPriority priority);
    Task InvokeAsync(Func<Task> action, DispatcherPriority priority);
}