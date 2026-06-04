using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Services.Repositiories;

public sealed class CurrentCharacterIdRepository : ICurrentCharacterIdRepository
{
    public Guid? Value { get; set; }
    public Guid Required => Value.Required;
}
