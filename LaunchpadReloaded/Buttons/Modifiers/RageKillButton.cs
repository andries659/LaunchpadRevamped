using Il2CppSystem;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers.Game.Crewmate;
using LaunchpadRevamped.Options.Modifiers.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Modifiers;

public class RageKillButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.rageKill";
    public override float Cooldown => OptionGroupSingleton<RageOptions>.Instance.KillCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.ShootButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        var local = PlayerControl.LocalPlayer;
        return local != null && local.HasModifier<RageModifier>();
    }

    // The button is shown from the start, but only works once every task is finished.
    public override bool CanUse()
    {
        return base.CanUse()
               && Target != null
               && PlayerControl.LocalPlayer.TryGetModifier<RageModifier>(out var rage)
               && rage.KillUnlocked;
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestPlayer(true, 1.1f);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Nullable<Color>(LaunchpadPalette.RageColor));
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
