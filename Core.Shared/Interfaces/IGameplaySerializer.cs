using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Statuses;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IGameplaySerializer
{
    void SerializeAbility(Ability value, Stream stream);
    Ability DeserializeAbility(Stream stream);
    void SerializeStatus(Status value, Stream stream);
    Status DeserializeStatus(Stream stream);
    void SerializePuppetStatusContext(PuppetStatusContext value, Stream stream);
    PuppetStatusContext DeserializePuppetStatusContext(Stream stream);
    void SerializeAbilityUseProgress(AbilityUseProgress value, Stream stream);
    AbilityUseProgress DeserializeAbilityUseProgress(Stream stream);
    void SerializeAbilitySlotState(AbilitySlotState value, Stream stream);
    AbilitySlotState DeserializeAbilitySlotState(Stream stream);
}
