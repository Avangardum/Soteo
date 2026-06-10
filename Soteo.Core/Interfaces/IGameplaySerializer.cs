using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Statuses;

namespace Soteo.Core.Interfaces;

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
