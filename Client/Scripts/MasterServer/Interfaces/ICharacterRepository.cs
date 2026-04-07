using Soteo.MasterServer.GameState.DataObjects;

namespace Soteo.MasterServer.Interfaces;

public interface ICharacterRepository : IDictionary<Guid, Character>
{
    void Add(Character character);
}