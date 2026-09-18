namespace Soteo.Core.Interfaces;

public interface IInitializationRepository
{
    bool IsInitialized { get; }
    Task WaitForInitAsync();
}
