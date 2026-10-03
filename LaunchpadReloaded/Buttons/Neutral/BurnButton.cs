using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Neutral;
using LaunchpadRevamped.Roles.Neutral;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Neutral;

public class BurnButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.burn";
    public override float Cooldown => OptionGroupSingleton<ArsonistOptions>.Instance.BurnCooldown;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.BurnButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ArsonistRole;
    }

    public override bool CanUse()
    {
        var needed = (int)OptionGroupSingleton<ArsonistOptions>.Instance.DousedNeededToBurn;
        return base.CanUse() && ArsonistUtilities.GetDousedPlayers().Length >= needed;
    }

    protected override void OnClick()
    {
        var arsonist = PlayerControl.LocalPlayer;

        foreach (var victim in ArsonistUtilities.GetDousedPlayers())
        {
            arsonist.RpcCustomMurder(victim, resetKillTimer: false, createDeadBody: true, teleportMurderer: false, showKillAnim: false, playKillSound: true);
        }

        // Host checks whether the Arsonist is now the last one standing.
        arsonist.RpcCheckArsonistWin();
    }
}
