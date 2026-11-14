using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Abilities;

public sealed class RecallAbility : Ability
{
    public override Targeting Targeting => Targeting.Nothing;
    public override bool LoopAnimation => true;
    public override double StaticUseTime => 10;

    protected override void TakeEffectCore(AbilityContext context)
    {
        context.User.Remove();
        context.Services.PacketSender
            .Send(new CharacterRecalledPacket { CharacterId = context.User.Id }, Const.CampaignServerId);
    }
}
