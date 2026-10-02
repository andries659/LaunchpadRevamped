using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Neutral;
using LaunchpadRevamped.Roles.Neutral;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using Helpers = LaunchpadRevamped.Utilities.Helpers;

namespace LaunchpadRevamped.Buttons.Neutral;

public class CollectButton : BaseLaunchpadButton<DeadBody>
{
    public override string Name => "launchpad.button.collect";
    public override float Cooldown => OptionGroupSingleton<ReaperOptions>.Instance.CollectCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.SoulButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => false;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ReaperRole;
    }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestObjectOfType<DeadBody>(Distance, Helpers.BodyFilter2D, "DeadBody", IsTargetValid);
    }

    public override bool IsTargetValid(DeadBody? target)
    {
        return target != null && target.GetCacheComponent() is
        {
            isReaped: false,
            hidden: false
        };
    }

    public override void SetOutline(bool active)
    {
        if (Target == null)
        {
            return;
        }

        foreach (var renderer in Target.bodyRenderers)
        {
            renderer.UpdateOutline(active ? PlayerControl.LocalPlayer.Data.Role.NameColor : null);
        }
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcCollectSoul(Target.ParentId);

        ResetTarget();
    }
}