using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Coven;

public class IllusionButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.illusion";
    public override float Cooldown => OptionGroupSingleton<IllusionistOptions>.Instance.IllusionCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => (int)OptionGroupSingleton<IllusionistOptions>.Instance.MaxIllusions;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.IllusionButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is IllusionistRole;
    }

    public override PlayerControl? GetTarget()
    {
        return CovenUtils.ClosestTarget(Distance, player => !IllusionistRole.IsIllusioned(player.PlayerId));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !target!.Data.IsDead && !IllusionistRole.IsIllusioned(target.PlayerId);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.IllusionistColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcIllusion(Target.PlayerId);
        ResetTarget();
    }
}
