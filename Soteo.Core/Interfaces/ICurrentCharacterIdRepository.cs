namespace Soteo.Core.Interfaces;

public interface ICurrentCharacterIdRepository
{
    Guid? Id { get; set; }
    Guid Required { get; }
}
