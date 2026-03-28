using Soteo.MasterServer.GameState.DataObjects;
using Soteo.MasterServer.Interfaces;

namespace Soteo.MasterServer.GameState.Repositories;

public sealed class CharacterRepository : Dictionary<Guid, Character>, ICharacterRepository
{
    public void Add(Character character) => Add(character.Id, character);
}