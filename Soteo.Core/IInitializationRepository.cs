namespace Soteo.Core.Interfaces;

public interface IInitializationRepository
{
    bool IsInitialized { get; set; }
    Task WaitForInitAsync();
}
