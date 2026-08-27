using Soteo.Core.Interfaces;

namespace Soteo.TestUtil;

public sealed class FakePauseRepo : IPauseRepository
{
    public bool IsPaused { get; set; }
}
