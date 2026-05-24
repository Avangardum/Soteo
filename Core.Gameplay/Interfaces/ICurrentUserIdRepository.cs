namespace Soteo.Core.Gameplay.Interfaces;

public interface ICurrentUserIdRepository
{
    Guid UserId { get; set; }
}