using Soteo.Core.Interfaces;

namespace Soteo.TestUtil;

public sealed class FakeInitializationRepo : IInitializationRepository
{
    public required bool IsInitialized { get; set; }
}
