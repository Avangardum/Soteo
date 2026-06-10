namespace Soteo.Core.Interfaces;

public interface IVisibleShardIdRepository
{
    Guid? Value { get; set; }
    Guid Required { get; }
}
