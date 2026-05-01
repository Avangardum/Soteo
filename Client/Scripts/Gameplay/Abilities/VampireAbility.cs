using Soteo.Gameplay.Statuses;
using Soteo.Gameplay.Util;

namespace Soteo.Gameplay.Abilities;

public sealed class VampireAbility : Ability
{
    public override Status PassiveStatus => Status.Instance<VampireStatus>();

    public override double PassiveTickInterval => 0.5f;

    public Scalable<double> LifestealFactor => [0.1, 0.2, 0.3, 0.4];
}