using System.Linq;
using HarmonyLib;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using UnityEngine;

namespace LaunchpadRevamped.Patches.Roles.Puppeteer;

/// <summary>
/// Runs on the Puppeteer's own client: a puppet that gets within kill range of a non-Impostor kills it
/// (credited to the Puppeteer) and is released.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class PuppeteerFixedUpdatePatch
{
    [HarmonyPostfix]
    public static void FixedUpdatePostfix(PlayerControl __instance)
    {
        if (!__instance.AmOwner || !__instance.Data || __instance.Data.IsDead || __instance.Data.Role is not PuppeteerRole
            || PuppeteerRole.Puppets.Count == 0)
        {
            return;
        }

        if (!ShipStatus.Instance || MeetingHud.Instance || ExileController.Instance)
        {
            return;
        }

        var killRange = AbilityUtils.KillRange();
        var all = PlayerControl.AllPlayerControls.ToArray();

        foreach (var puppetId in PuppeteerRole.Puppets.ToArray())
        {
            var puppet = AbilityUtils.GetPlayer(puppetId);
            if (!puppet || !puppet!.Data || puppet.Data.IsDead || puppet.Data.Disconnected)
            {
                PuppeteerRole.Puppets.Remove(puppetId);
                continue;
            }

            if (!puppet.CanMove)
            {
                continue;
            }

            var victim = all
                .Where(player => player && player != puppet && player.Data && !player.Data.IsDead && !player.Data.Disconnected
                                 && !player.Data.Role.IsImpostor && player.CanMove)
                .OrderBy(player => Vector2.Distance(puppet.GetTruePosition(), player.GetTruePosition()))
                .FirstOrDefault();

            if (!victim || Vector2.Distance(puppet.GetTruePosition(), victim!.GetTruePosition()) > killRange)
            {
                continue;
            }

            PuppeteerRole.Puppets.Remove(puppetId);
            __instance.RpcCustomMurder(victim, resetKillTimer: false, teleportMurderer: false, showKillAnim: false);

            if (OptionGroupSingleton<PuppeteerOptions>.Instance.DoubleKills)
            {
                __instance.RpcCustomMurder(puppet, resetKillTimer: false, teleportMurderer: false, showKillAnim: false);
            }
        }
    }
}

public static class PuppeteerEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            PuppeteerRole.Puppets.Clear();
        }
    }

    // Puppets are released the moment a meeting starts (TOHO clears them when a body or button is reported).
    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent @event)
    {
        PuppeteerRole.Puppets.Clear();
    }
}
