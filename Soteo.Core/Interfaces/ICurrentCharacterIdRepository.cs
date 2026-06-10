namespace Soteo.Core.Gameplay.Interfaces;

public interface ICurrentCharacterIdRepository
{
    Guid? Value { get; set; }
    Guid Required { get; }
}
