using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

public sealed class CurrentCharacterIdRepository : ICurrentCharacterIdRepository
{
    public Guid? Id { get; set; }
}
