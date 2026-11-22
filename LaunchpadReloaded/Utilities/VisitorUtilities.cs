using System.Collections.Generic;
using System.Linq;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Utilities;

/// <summary>
/// Everything the Visitor tracks. All of it lives on the Visitor's own client: positions and task progress of every player
/// are already known to every client, so no networking is needed. A visit lasts until the next meeting, where the
/// report is posted in the chat.
/// </summary>
public static class VisitorUtilities
{
    private sealed class VisitData
    {
        public readonly List<string> Rooms = new();
        public string? LastRoom;
        public int TasksAtStart;
        public int KillsAtStart;
        public string? KillerInfo;
    }

    private static readonly Dictionary<byte, VisitData> Visits = new();

    // Kills are counted on every client so a visit can report how many kills the target made.
    private static readonly Dictionary<byte, int> KillCounts = new();

    public static bool IsVisiting(PlayerControl target) => Visits.ContainsKey(target.PlayerId);

    public static void Reset()
    {
        Visits.Clear();
    }

    public static void StartVisit(PlayerControl target)
    {
        Visits[target.PlayerId] = new VisitData
        {
            TasksAtStart = CompletedTasks(target),
            KillsAtStart = KillCounts.GetValueOrDefault(target.PlayerId),
        };

        Say(MiraLocaleManager.GetParsed("launchpad.visitor.visiting", new() { ["{name}"] = target.Data.PlayerName }));
    }

    /// <summary>Records the rooms of visited players. Called from the Visit button's fixed update.</summary>
    public static void Tick()
    {
        if (Visits.Count == 0 || ShipStatus.Instance == null)
        {
            return;
        }

        foreach (var (id, data) in Visits)
        {
            var target = GetPlayer(id);
            if (target == null || target.Data == null || target.Data.IsDead)
            {
                continue;
            }

            var room = GetRoomName(target);
            if (room == null || room == data.LastRoom)
            {
                continue;
            }

            data.LastRoom = room;
            data.Rooms.Add(room);
        }
    }

    [RegisterEvent]
    public static void RoundStartEvent(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            Visits.Clear();
            KillCounts.Clear();
        }
    }

    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        KillCounts[@event.Source.PlayerId] = KillCounts.GetValueOrDefault(@event.Source.PlayerId) + 1;

        if (!Visits.TryGetValue(@event.Target.PlayerId, out var data))
        {
            return;
        }

        if (!OptionGroupSingleton<VisitorOptions>.Instance.RevealKiller)
        {
            return;
        }

        data.KillerInfo = MiraLocaleManager.GetParsed("launchpad.visitor.killerReveal", new()
        {
            ["{name}"] = @event.Target.Data.PlayerName,
            ["{killer}"] = @event.Source.Data.PlayerName,
            ["{role}"] = @event.Source.Data.Role.NiceName,
        });

        Say(data.KillerInfo);
    }

    [RegisterEvent]
    public static void StartMeetingEvent(StartMeetingEvent @event)
    {
        if (Visits.Count == 0)
        {
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (local == null || local.Data == null || local.Data.IsDead || local.Data.Role is not VisitorRole)
        {
            Visits.Clear();
            return;
        }

        foreach (var (id, data) in Visits)
        {
            var target = GetPlayer(id);
            var name = target?.Data?.PlayerName ?? "?";
            var rooms = data.Rooms.Count > 0
                ? string.Join(", ", data.Rooms)
                : MiraLocaleManager.Get("launchpad.visitor.noRooms");
            var tasks = target != null ? System.Math.Max(0, CompletedTasks(target) - data.TasksAtStart) : 0;
            var kills = System.Math.Max(0, KillCounts.GetValueOrDefault(id) - data.KillsAtStart);

            Say(MiraLocaleManager.GetParsed("launchpad.visitor.report", new()
            {
                ["{name}"] = name,
                ["{rooms}"] = rooms,
                ["{tasks}"] = tasks.ToString(),
                ["{kills}"] = kills.ToString(),
            }));

            if (data.KillerInfo != null)
            {
                Say(data.KillerInfo);
            }
        }

        Visits.Clear();
    }

    private static void Say(string text)
    {
        if (HudManager.Instance == null || PlayerControl.LocalPlayer == null)
        {
            return;
        }

        HudManager.Instance.Chat.AddChat(PlayerControl.LocalPlayer, $"<color=#{ColorUtility.ToHtmlStringRGB(LaunchpadPalette.VisitorColor)}>{text}</color>");
    }

    private static PlayerControl? GetPlayer(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.PlayerId == id);
    }

    private static int CompletedTasks(PlayerControl player)
    {
        return player.Data.Tasks.ToArray().Count(t => t.Complete);
    }

    private static string? GetRoomName(PlayerControl player)
    {
        Vector2 position = player.GetTruePosition();
        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (room.roomArea != null && room.roomArea.OverlapPoint(position))
            {
                return TranslationController.Instance.GetString(room.RoomId);
            }
        }

        return null;
    }
}
