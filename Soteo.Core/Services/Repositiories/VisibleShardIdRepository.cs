using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositiories;

public sealed class VisibleShardIdRepository : IVisibleShardIdRepository
{
    public Guid? Value { get; set; }
    public Guid Required => Value.Required;
}
