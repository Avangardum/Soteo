namespace Soteo.Core.Client;

public sealed class CurrentCharacterIdRepository : ICurrentCharacterIdRepository
{
    public Guid? Id { get; set; }
}
