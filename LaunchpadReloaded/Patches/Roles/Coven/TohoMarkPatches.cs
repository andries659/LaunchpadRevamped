using HarmonyLib;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Roles.Impostor;
using UnityEngine;

namespace LaunchpadRevamped.Patches.Roles.Coven;

/// <summary>
/// Colours the names of players that carry one of the marks of the roles ported from TOHO, for the players who are
/// allowed to see them: puppets (Puppeteer), bitten players (Vampire), jinxed and illusioned players (the Jinx,
/// the Illusionist and the rest of the Coven).
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class TohoMarkHighlightPatch
{
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerControl __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || !__instance.cosmetics || !__instance.cosmetics.nameText)
        {
            return;
        }

        var id = __instance.PlayerId;
        var role = local.Data.Role;
        var coven = role is ICovenRole;

        Color color;
        if (role is PuppeteerRole && PuppeteerRole.Puppets.Contains(id))
        {
            color = LaunchpadPalette.PuppeteerColor;
        }
        else if (role is VampireRole && VampireRole.Bitten.Contains(id))
        {
            color = LaunchpadPalette.VampireColor;
        }
        else if ((role is JinxRole || coven) && JinxRole.Jinxed.ContainsKey(id))
        {
            color = LaunchpadPalette.JinxColor;
        }
        else if ((role is IllusionistRole || coven) && IllusionistRole.IsIllusioned(id))
        {
            color = LaunchpadPalette.IllusionistColor;
        }
        else
        {
            return;
        }

        __instance.cosmetics.nameText.color = color;
    }
}
