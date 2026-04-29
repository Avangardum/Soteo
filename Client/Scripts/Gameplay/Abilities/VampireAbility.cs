using Soteo.Gameplay.Statuses;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Abilities;

public sealed class VampireAbility : Ability
{
    public override Status PassiveStatus => Status.Instance<VampireStatus>();
    public Scalable<float> LifestealFactor => [0.1f, 0.2f, 0.3f, 0.4f];
}