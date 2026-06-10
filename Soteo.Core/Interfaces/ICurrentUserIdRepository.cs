namespace Soteo.Core.Gameplay.Interfaces;

public interface ICurrentUserIdRepository
{
    Guid? Value { get; set; }
    Guid Required { get; }
}
