using Soteo.Core.Interfaces;

namespace Soteo.TestUtil;

public sealed class FakeInitializationRepo : IInitializationRepository
{
    public required bool Initialized { get; set; }
}
