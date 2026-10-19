namespace Soteo.Core.Interfaces;

public interface IVisibleShardIdRepository
{
    Guid? Value { get; set; } // todo rename
    Guid Required { get; } // todo remove
}
