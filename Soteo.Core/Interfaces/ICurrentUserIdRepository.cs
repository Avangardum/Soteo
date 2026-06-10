namespace Soteo.Core.Interfaces;

public interface ICurrentUserIdRepository
{
    Guid? Value { get; set; }
    Guid Required { get; }
}
