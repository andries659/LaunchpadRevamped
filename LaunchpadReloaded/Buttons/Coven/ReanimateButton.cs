using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using Helpers = LaunchpadRevamped.Utilities.Helpers;

namespace LaunchpadRevamped.Buttons.Coven;

public class ReanimateButton : BaseLaunchpadButton<DeadBody>
{
    public override string Name => "launchpad.button.reanimate";
    public override float Cooldown => OptionGroupSingleton<NecromancerOptions>.Instance.ReanimateCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<NecromancerOptions>.Instance.Reanimations;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.ReanimateButton;
    public override float Distance => PlayerControl.LocalPlayer.MaxReportDistance / 4f;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is NecromancerRole;
    }

    public override void SetOutline(bool active)
    {
        if (Target == null)
        {
            return;
        }

        foreach (var renderer in Target.bodyRenderers)
        {
            renderer.UpdateOutline(active ? LaunchpadPalette.NecromancerColor : null);
        }
    }

    public override DeadBody? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetNearestObjectOfType<DeadBody>(Distance, Helpers.BodyFilter2D, "DeadBody", IsTargetValid);
    }

    public override bool IsTargetValid(DeadBody? target)
    {
        return target != null && target.GetCacheComponent() is
        {
            hidden: false
        };
    }

    public override bool CanUse()
    {
        return base.CanUse() && Target &&
               !PlayerControl.LocalPlayer.Data.IsDead &&
               PlayerControl.LocalPlayer.CanMove &&
               !PlayerControl.LocalPlayer.HasModifier<DragBodyModifier>();
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcReanimate(Target.ParentId);
        ResetTarget();
    }
}
