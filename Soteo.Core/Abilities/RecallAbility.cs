using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.Abilities;

public sealed class RecallAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Nothing;
    public override bool LoopAnimation => true;
    public override Scalable<double> StaticUseTime => 10;
    
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.Remove();
        context.GetRequiredService<IFromGameplayPacketSender>()
            .SendReliable(new CharacterRecalledPacket { CharacterId = context.User.Id }, Const.CampaignServerId);
    }
}
