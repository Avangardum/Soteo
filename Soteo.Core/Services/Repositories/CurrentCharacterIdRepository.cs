using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

public sealed class CurrentCharacterIdRepository : ICurrentCharacterIdRepository
{
    public Guid? Id { get; set; } // todo rename
    public Guid Required => Id.Required; // todo remove
}
