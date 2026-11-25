using HarmonyLib;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Patches.Roles.Blackmailer;

/// <summary>
/// A blackmailed player cannot speak in the meeting.
/// </summary>
[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class BlackmailChatPatch
{
    [HarmonyPrefix]
    public static bool SendChatPrefix()
    {
        var local = PlayerControl.LocalPlayer;
        if (!local || !MeetingHud.Instance || !BlackmailerRole.IsBlackmailed(local.PlayerId))
        {
            return true;
        }

        MiraAPI.Utilities.Helpers.CreateAndShowNotification(
            MiraLocaleManager.Get("launchpad.blackmailer.silenced"), LaunchpadPalette.BlackmailerColor);
        return false;
    }
}

public static class BlackmailerEvents
{
    [RegisterEvent]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        // Blackmail lasts for one meeting: it is cleared when the meeting ends, and at the start of a game.
        BlackmailerRole.Blackmailed.Clear();
    }

    [RegisterEvent]
    public static void OnMeetingStart(StartMeetingEvent @event)
    {
        if (BlackmailerRole.Blackmailed.Count == 0)
        {
            return;
        }

        var names = new System.Collections.Generic.List<string>();

        foreach (var state in @event.MeetingHud.playerStates)
        {
            if (!BlackmailerRole.IsBlackmailed(state.PlayerId))
            {
                continue;
            }

            // Everybody sees the cross next to the name, like TOHO.
            if (state.NameText)
            {
                state.NameText.text += $" <color=#{UnityEngine.ColorUtility.ToHtmlStringRGB(LaunchpadPalette.BlackmailerColor)}>╳</color>";
            }

            var data = GameData.Instance.GetPlayerById(state.PlayerId);
            if (data != null)
            {
                names.Add(data.PlayerName);
            }
        }

        if (AmongUsClient.Instance.AmHost && names.Count > 0)
        {
            RpcWelcomeMessage.Broadcast(MiraLocaleManager.GetParsed("launchpad.blackmailer.announce",
                new[] { ("{players}", string.Join(", ", names)) }));
        }
    }
}
