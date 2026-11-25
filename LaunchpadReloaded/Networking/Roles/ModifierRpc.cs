using AmongUs.GameOptions;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Utilities;
using MiraAPI.Translation;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LaunchpadRevamped.Networking.Roles;

/// <summary>
/// Feedback for the modifiers that stop or spoil a kill. Sent by the host, which is the one that decides kills.
/// </summary>
public static class ModifierRpc
{
    public const byte KindLucky = 0;
    public const byte KindExtraLife = 1;

    /// <summary>The target's modifier stopped an attack. The attacker gets a fresh cooldown and a notice.</summary>
    [MethodRpc((uint)LaunchpadRpc.ModifierBlock)]
    public static void RpcModifierBlock(this PlayerControl target, PlayerControl attacker, byte kind)
    {
        var key = kind == KindLucky ? "launchpad.modifier.lucky.dodged" : "launchpad.modifier.extraLife.used";

        if (attacker && attacker.AmOwner)
        {
            attacker.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(MiraLocaleManager.Get(key + ".attacker"), UnityEngine.Color.white);
        }

        if (target && target.AmOwner)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(MiraLocaleManager.Get(key), LaunchpadPalette.BaitColor);
        }
    }

    /// <summary>The killer's Clumsy modifier made them miss.</summary>
    [MethodRpc((uint)LaunchpadRpc.KillMissed)]
    public static void RpcKillMissed(this PlayerControl killer)
    {
        if (killer && killer.AmOwner)
        {
            killer.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(MiraLocaleManager.Get("launchpad.modifier.clumsy.missed"), UnityEngine.Color.white);
        }
    }
}
