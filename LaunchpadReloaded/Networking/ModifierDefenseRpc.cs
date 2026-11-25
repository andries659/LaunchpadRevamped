using LaunchpadRevamped.Utilities;
using MiraAPI.Translation;
using Reactor.Networking.Attributes;
using UnityEngine;
using Color = UnityEngine.Color;

namespace LaunchpadRevamped.Networking;

/// <summary>
/// Feedback for the modifiers that stop or spoil a kill (Clumsy, Lucky, Extra Life). Sent by the host, because the host
/// is the one that decides. The attacker's own client gets a fresh kill cooldown.
/// </summary>
public static class ModifierDefenseRpc
{
    // Kept well away from the LaunchpadRpc enum so the two can never collide.
    private const uint DefenseRpcId = 9101;

    public const byte Miss = 0;
    public const byte Dodge = 1;
    public const byte ExtraLife = 2;

    [MethodRpc(DefenseRpcId)]
    public static void RpcModifierDefense(this PlayerControl defender, PlayerControl attacker, byte kind)
    {
        if (attacker && attacker.AmOwner)
        {
            attacker.SetKillTimer(ModifierHelpers.BaseKillCooldown);
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(kind switch
            {
                Miss => MiraLocaleManager.Get("launchpad.modifier.clumsy.missed", "Your kill missed!"),
                Dodge => MiraLocaleManager.Get("launchpad.modifier.lucky.dodged", "They dodged your attack!"),
                _ => MiraLocaleManager.Get("launchpad.modifier.extraLife.used", "They had an extra life!"),
            }, Color.yellow);
        }

        if (defender && defender.AmOwner && kind != Miss)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(kind == Dodge
                ? MiraLocaleManager.Get("launchpad.modifier.lucky.youDodged", "You dodged an attack!")
                : MiraLocaleManager.Get("launchpad.modifier.extraLife.youUsed", "You lost your extra life!"), Color.green);
        }
    }
}
