using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Abilities;

public sealed class RecallAbility : Ability
{
    public override Targeting Targeting => Targeting.Nothing;
    public override bool LoopAnimation => true;
    public override double StaticUseTime => 10;

    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.Remove();
        context.GetRequiredService<IFromGameplayPacketSender>()
            .Send(new CharacterRecalledPacket { CharacterId = context.User.Id }, Const.CampaignServerId);
    }
}
