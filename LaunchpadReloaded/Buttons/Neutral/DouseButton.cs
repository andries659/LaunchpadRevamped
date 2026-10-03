using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Options.Roles.Neutral;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Neutral;

public class DouseButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.douse";
    public override float Cooldown => OptionGroupSingleton<ArsonistOptions>.Instance.DouseCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.DouseButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ArsonistRole;
    }

    public override PlayerControl? GetTarget()
    {
        // Skip players that are already doused so the button targets the nearest fresh victim.
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, 1.1f, predicate: player => !player.HasModifier<DousedModifier>());
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !target!.Data.IsDead && !target.HasModifier<DousedModifier>();
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.ArsonistColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        Target.RpcAddModifier<DousedModifier>();
        ResetTarget();
    }
}
