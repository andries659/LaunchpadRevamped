using HarmonyLib;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Networking;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Sends the "onmeeting:" block of LPR_DATA/Welcome/welcome.txt to everyone's chat when a meeting starts.
/// Only the host does this, so the message appears once per player. It reads the file every time, so edits show
/// up in the next meeting without restarting the game.
/// </summary>
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class MeetingMessagePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var message = LprData.ReadOnMeetingMessage();
        if (message != null)
        {
            RpcWelcomeMessage.Broadcast(message);
        }
    }
}
