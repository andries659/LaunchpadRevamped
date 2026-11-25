using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

public class BlackmailButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.blackmail";
    public override float Cooldown => OptionGroupSingleton<BlackmailerOptions>.Instance.BlackmailCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.BlackmailButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    // The vanilla kill button stays, so the ability goes on the secondary key.
    public override BaseKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is BlackmailerRole;
    }

    public override PlayerControl? GetTarget()
    {
        return AbilityUtils.ClosestNonImpostor(Distance, player => !BlackmailerRole.IsBlackmailed(player.PlayerId));
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !target!.Data.IsDead && !BlackmailerRole.IsBlackmailed(target.PlayerId);
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.BlackmailerColor));
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            return;
        }

        PlayerControl.LocalPlayer.RpcBlackmail(Target.PlayerId);
        ResetTarget();
    }
}
