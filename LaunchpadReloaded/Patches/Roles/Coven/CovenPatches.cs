using HarmonyLib;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using System.Linq;
using UnityEngine;

namespace LaunchpadRevamped.Patches.Roles.Coven;

/// <summary>
/// The Hex Master sees which players are currently hexed.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class HexHighlightPatch
{
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerControl __instance)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || local.Data.Role is not HexMasterRole)
        {
            return;
        }

        if (HexMasterRole.Hexed.Contains(__instance.PlayerId) && __instance.cosmetics && __instance.cosmetics.nameText)
        {
            __instance.cosmetics.nameText.color = LaunchpadPalette.HexMasterColor;
        }
    }
}

/// <summary>
/// Host-side check for the Coven win condition, a couple of times per second.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class CovenWinPatch
{
    private const float CheckInterval = 0.5f;

    private static float _nextCheck;
    private static bool _triggered;

    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerControl __instance)
    {
        if (!ShipStatus.Instance)
        {
            _triggered = false;
            return;
        }

        if (_triggered || !__instance.AmOwner || !AmongUsClient.Instance.AmHost
            || MeetingHud.Instance || ExileController.Instance || Time.unscaledTime < _nextCheck)
        {
            return;
        }

        _nextCheck = Time.unscaledTime + CheckInterval;

        // Cheap early out: no Coven in this game.
        if (!PlayerControl.AllPlayerControls.ToArray().Any(player => CovenUtils.IsCoven(player)))
        {
            return;
        }

        if (CovenUtils.TryTriggerCovenWin())
        {
            _triggered = true;
        }
    }
}
