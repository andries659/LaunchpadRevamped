using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Coven;

public class HexButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.hex";
    public override float Cooldown => OptionGroupSingleton<HexMasterOptions>.Instance.HexCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.HexButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HexMasterRole;
    }

    public override PlayerControl? GetTarget()
    {
        return CovenUtils.ClosestTarget(Distance, player => !HexMasterRole.Hexed.Contains(player.PlayerId));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !HexMasterRole.Hexed.Contains(target!.PlayerId);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.HexMasterColor));
    }

    public override bool CanUse()
    {
        return base.CanUse() && HexMasterRole.Hexed.Count < (int)OptionGroupSingleton<HexMasterOptions>.Instance.MaxHexes;
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        HexMasterRole.Hexed.Add(Target.PlayerId);
        ResetTarget();
    }
}
