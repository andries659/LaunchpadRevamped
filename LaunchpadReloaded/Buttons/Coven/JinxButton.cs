using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Coven;

public class JinxButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.jinx";
    public override float Cooldown => OptionGroupSingleton<JinxOptions>.Instance.JinxCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<JinxOptions>.Instance.MaxJinxes;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.JinxButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is JinxRole;
    }

    public override PlayerControl? GetTarget()
    {
        return CovenUtils.ClosestTarget(Distance, player => !JinxRole.Jinxed.ContainsKey(player.PlayerId));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !target!.Data.IsDead && !JinxRole.Jinxed.ContainsKey(target.PlayerId);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.JinxColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcJinx(Target.PlayerId, true);
        ResetTarget();
    }
}
