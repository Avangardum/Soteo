namespace Soteo.Core.Gameplay.Interfaces;

public interface IVisibleShardIdRepository
{
    Guid? Value { get; set; }
    Guid Required { get; }
}
