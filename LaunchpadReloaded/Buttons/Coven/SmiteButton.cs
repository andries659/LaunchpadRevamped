using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Coven;

public class SmiteButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.smite";
    public override float Cooldown => OptionGroupSingleton<CovenLeaderOptions>.Instance.SmiteCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.SmiteButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is CovenLeaderRole;
    }

    public override PlayerControl? GetTarget()
    {
        return CovenUtils.ClosestTarget(Distance);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.CovenColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        ResetTarget();
    }
}
