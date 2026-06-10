namespace Soteo.Core.Interfaces;

public interface ICurrentCharacterIdRepository
{
    Guid? Value { get; set; }
    Guid Required { get; }
}
