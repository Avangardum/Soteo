using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

public sealed class CurrentTickRepository : ICurrentTickRepository
{
    public long Value { get; set; }
}
