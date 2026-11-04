using System.Linq;
using HarmonyLib;
using InnerNet;
using LaunchpadRevamped.Features.TestClients;
using UnityEngine;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Test clients (fake players for solo testing). Local games only.
/// Lobby: F5 add bot, F6 keep/lose bots between games, F11 remove all bots.
/// Anywhere: F9 / F10 switch which player you control.
/// </summary>
[HarmonyPatch]
public static class TestClientPatches
{
    private static int _lastKeyFrame = -1;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
    public static void LobbyStartPostfix()
    {
        TestClientManager.Enabled = AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame;
        if (TestClientManager.Enabled)
        {
            TestClientManager.OnLobbyLoaded();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
    public static void KeyboardPostfix()
    {
        if (!TestClientManager.Enabled || !AmongUsClient.Instance || !PlayerControl.LocalPlayer)
        {
            return;
        }

        // KeyboardJoystick can exist more than once (one per player), so only react once per frame.
        if (_lastKeyFrame == Time.frameCount)
        {
            return;
        }

        _lastKeyFrame = Time.frameCount;

        if (Input.GetKeyDown(KeyCode.F5) && LobbyBehaviour.Instance)
        {
            TestClientManager.CreateBot();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            TestClientManager.KeepBots = !TestClientManager.KeepBots;
            Info($"Test bots kept between games: {TestClientManager.KeepBots}");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            TestClientManager.SwitchRelative(1);
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            TestClientManager.SwitchRelative(-1);
        }

        if (Input.GetKeyDown(KeyCode.F11) && TestClientManager.HasBots)
        {
            TestClientManager.RemoveAllBots();
        }
    }

    // Skip the start countdown while testing.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public static void CountdownPrefix(GameStartManager __instance)
    {
        if (TestClientManager.Enabled && TestClientManager.HasBots)
        {
            __instance.countDownTimer = 0;
        }
    }

    // Airship: nobody controls the bots, so they pick a random spawn point for themselves.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Begin))]
    public static void AirshipSpawnPostfix(SpawnInMinigame __instance)
    {
        if (!TestClientManager.Enabled)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || !TestClientManager.IsBot(player.PlayerId) || player == PlayerControl.LocalPlayer)
            {
                continue;
            }

            var spawn = __instance.Locations[Random.Range(0, __instance.Locations.Count)];
            player.gameObject.SetActive(true);
            player.NetTransform.RpcSnapTo(spawn.Location);
        }
    }

    // Bots you aren't controlling skip-vote, so meetings finish as soon as you've voted.
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public static void BotVotePostfix(MeetingHud __instance)
    {
        if (!TestClientManager.Enabled || !TestClientManager.HasBots || __instance.state != MeetingHud.MeetingStates.NotVoted)
        {
            return;
        }

        foreach (var area in __instance.playerStates)
        {
            if (area.AmDead || area.DidVote || !TestClientManager.IsBot(area.PlayerId) ||
                area.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                continue;
            }

            __instance.CmdCastVote(area.PlayerId, PlayerVoteArea.SkippedVote);
        }
    }

    // Bots you aren't controlling stay put instead of following your keyboard input.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static void FreezeBotsPrefix(PlayerPhysics __instance)
    {
        var player = __instance.myPlayer;
        if (TestClientManager.Enabled && player && player != PlayerControl.LocalPlayer &&
            TestClientManager.IsBot(player.PlayerId))
        {
            player.moveable = false;
        }
    }
}
