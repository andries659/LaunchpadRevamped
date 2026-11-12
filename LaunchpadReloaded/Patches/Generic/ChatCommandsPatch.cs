using HarmonyLib;
using LaunchpadRevamped.Features;

namespace LaunchpadRevamped.Patches.Generic;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
public static class ChatCommandPatch
{
    private const string SenderName = "<color=#FFD54A>SYSTEM MESSAGE</color>";

    public static bool Prefix(ChatController __instance)
    {
        // Placeholders are resolved here, before the name swap below.
        var lines = ChatCommands.Run(__instance.freeChatField.Text);
        if (lines == null)
        {
            return true; // normal chat message
        }

        __instance.freeChatField.Clear();

        // Shown only to you, nothing is sent to other players.
        var data = PlayerControl.LocalPlayer.Data;
        var original = data.PlayerName;

        try
        {
            data.PlayerName = SenderName;

            foreach (var line in lines)
            {
                __instance.AddChat(PlayerControl.LocalPlayer, line);
            }
        }
        finally
        {
            data.PlayerName = original;
        }

        return false;
    }
}
