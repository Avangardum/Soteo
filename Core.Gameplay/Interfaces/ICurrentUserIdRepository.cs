namespace Soteo.Gameplay.Interfaces;

public interface ICurrentUserIdRepository
{
    Guid UserId { get; set; }
}