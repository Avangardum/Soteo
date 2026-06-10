using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Abilities;

public sealed class VampireAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Passive;
    public override string IconPath => "Placeholder2";
    public override Status PassiveStatus => Status.Instance<VampireStatus>();
    public override double? PassiveTickInterval => 0.5;
    public Scalable<double> LifestealFactor => [0.1, 0.2, 0.3, 0.4];
}
