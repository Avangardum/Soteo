using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

public sealed class CurrentCharacterIdRepository : ICurrentCharacterIdRepository
{
    public Guid? Value { get; set; } // todo rename
    public Guid Required => Value.Required; // todo remove
}
