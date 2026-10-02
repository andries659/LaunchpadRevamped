using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LaunchpadRevamped.Buttons.Impostor;

public class HackButton : BaseLaunchpadButton
{
    public override string Name => "launchpad.button.hack";
    public override float Cooldown => (int)OptionGroupSingleton<HackerOptions>.Instance.HackCooldown;
    public override float EffectDuration => OptionGroupSingleton<HackerOptions>.Instance.HackDuration;
    public override int MaxUses => (int)OptionGroupSingleton<HackerOptions>.Instance.HackUses;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.HackButton;
    public override bool TimerAffectedByPlayer => true;
    public override bool AffectedByHack => false;
    public override bool Enabled(RoleBehaviour? role) => role is HackerRole;
    public override bool CanUse() => base.CanUse() && !HackerUtilities.AnyPlayerHacked();

    protected override void OnClick()
    {
        RpcHackPlayers(PlayerControl.LocalPlayer);
    }

    [MethodRpc((ushort)LaunchpadRpc.Hack)]
    public static void RpcHackPlayers(PlayerControl hacker)
    {
        if (hacker.Data.Role is not HackerRole)
        {
            hacker.KickForCheating();
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player.Data.IsDead || player.Data.Disconnected)
            {
                continue;
            }

            player.AddModifier<HackedModifier>();
        }
    }
}