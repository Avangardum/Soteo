using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Gameplay.Services;

public sealed class CurrentCharacterIdRepository : ICurrentCharacterIdRepository
{
    public Guid? Value { get; set; }
    public Guid Required => Value.Required;
}
