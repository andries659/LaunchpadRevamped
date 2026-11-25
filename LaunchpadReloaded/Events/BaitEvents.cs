using System.Collections;
using System.Linq;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Modifiers.Game.Universal;
using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Modifiers.Universal;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Events;

/// <summary>
/// Ported from TOHO (Bait). The killer's own client reports the body after the configured delay.
/// </summary>
public static class BaitEvents
{
    // TOHO never lets the delay drop below this, so the kill animation can finish first.
    private const float MinimumDelay = 0.15f;

    private static bool _firstMeetingDone;

    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            _firstMeetingDone = false;
        }
    }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        var killer = @event.Source;
        var victim = @event.Target;

        if (!killer || !victim || killer == victim || !killer.AmOwner || !victim.HasModifier<BaitModifier>())
        {
            return;
        }

        Coroutines.Start(CoSelfReport(killer, victim));
    }

    private static IEnumerator CoSelfReport(PlayerControl killer, PlayerControl victim)
    {
        var options = OptionGroupSingleton<BaitOptions>.Instance;
        var min = (int)options.DelayMin;
        var max = (int)options.DelayMax;
        var delay = max < min ? 0f : Random.Range(min, max + 1);
        delay = Mathf.Max(delay, MinimumDelay);

        if (delay > MinimumDelay && options.NotifyKiller)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification(
                MiraLocaleManager.GetParsed("launchpad.modifier.bait.notify", new[] { ("{seconds}", ((int)delay).ToString()) }),
                LaunchpadPalette.BaitColor);
        }

        var elapsed = 0f;
        while (elapsed < delay)
        {
            // A meeting started by someone else, or the killer died, ends the forced report.
            if (!killer || killer.Data.IsDead || MeetingHud.Instance || !ShipStatus.Instance)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (killer && !killer.Data.IsDead && victim && !MeetingHud.Instance && ShipStatus.Instance)
        {
            killer.CmdReportDeadBody(victim.Data);
        }
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent @event)
    {
        if (_firstMeetingDone)
        {
            return;
        }

        _firstMeetingDone = true;

        if (!AmongUsClient.Instance.AmHost || !OptionGroupSingleton<BaitOptions>.Instance.ShowAliveBaits)
        {
            return;
        }

        var names = PlayerControl.AllPlayerControls.ToArray()
            .Where(player => player && player.Data && !player.Data.IsDead && !player.Data.Disconnected && player.HasModifier<BaitModifier>())
            .Select(player => player.Data.PlayerName)
            .ToArray();

        if (names.Length == 0)
        {
            return;
        }

        RpcWelcomeMessage.Broadcast(MiraLocaleManager.GetParsed("launchpad.modifier.bait.aliveList",
            new[] { ("{players}", string.Join(", ", names)) }));
    }
}
