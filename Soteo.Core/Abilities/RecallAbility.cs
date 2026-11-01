using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

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
