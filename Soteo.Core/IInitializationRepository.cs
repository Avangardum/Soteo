namespace Soteo.Core;

public interface IInitializationRepository
{
    bool IsInitialized { get; set; }
    Task WaitForInitAsync();
}
