using HarmonyLib;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Impostor;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Patches.Roles.Esper;

/// <summary>
/// Runs on every client when a meeting starts (before the MeetingHud exists): any projected Esper dies.
/// This covers both "meeting called while projected" and "body reported while projected".
/// </summary>
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
public static class EsperMeetingPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void StartMeetingPrefix()
    {
        EsperProjection.KillAllProjected();
    }
}

/// <summary>
/// Reduced vision while projected.
/// </summary>
[HarmonyPatch(typeof(AirshipStatus), nameof(AirshipStatus.CalculateLightRadius))]
[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CalculateLightRadius))]
public static class EsperVisionPatch
{
    public static void Postfix(NetworkedPlayerInfo player, ref float __result)
    {
        if (!player || !player.Object || !EsperProjection.IsProjected(player.Object))
        {
            return;
        }

        __result *= OptionGroupSingleton<EsperOptions>.Instance.ProjectionVision;
    }
}

/// <summary>
/// The kill animation freezes the killer with SetMovement(false) and unfreezes it with SetMovement(true), which also
/// switches the collider back on. Keep it off for a projected Esper so it can still walk through walls after a kill.
/// </summary>
[HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.SetMovement))]
public static class EsperKillMovementPatch
{
    public static void Postfix(PlayerControl source, bool canMove)
    {
        if (canMove && source && source.AmOwner && EsperProjection.IsProjected(source))
        {
            source.Collider.enabled = false;
        }
    }
}
