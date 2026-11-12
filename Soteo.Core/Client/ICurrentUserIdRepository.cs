namespace Soteo.Core.Client;

public interface ICurrentUserIdRepository
{
    Guid? Id { get; set; }
}
