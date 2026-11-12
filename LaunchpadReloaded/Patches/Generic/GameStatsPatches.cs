using HarmonyLib;
using LaunchpadRevamped.Features;

namespace LaunchpadRevamped.Patches.Generic;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
public static class GameStatsShipStartPatch
{
    public static void Postfix() => ChatCommands.GameStarted();
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
public static class GameStatsMeetingPatch
{
    public static void Postfix() => ChatCommands.MeetingStarted();
}

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class GameStatsLobbyPatch
{
    public static void Postfix() => ChatCommands.BackInLobby();
}
