using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

public sealed class CurrentCharacterIdRepository : ICurrentCharacterIdRepository
{
    public Guid? Value { get; set; }
    public Guid Required => Value.Required;
}
