using System;
using System.Collections;
using System.IO;
using Discord;
using HarmonyLib;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Custom Discord RPC
/// </summary>
[HarmonyPatch]
public static class DiscordManagerPatch
{
    private const long ClientId = 1569080104464814302;
    private const uint SteamAppId = 945360;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DiscordManager), nameof(DiscordManager.Start))]
    public static bool DiscordManagerStartPrefix(DiscordManager __instance)
    {
        DiscordManager.ClientId = ClientId;
        if (OperatingSystem.IsAndroid())
        {
            return true;
        }

        InitializeDiscord(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
    public static void ActivityManagerUpdateActivityPrefix(ActivityManager __instance, [HarmonyArgument(0)] ref Activity activity)
    {
        activity.Details += " ★ Launchpad: Revamped ★";

        // In a lobby the status shows the room code and how full it is.
        if (TryGetLobbyInfo(out var lobbyInfo))
        {
            activity.State = lobbyInfo;
        }
        else
        {
            activity.State += " | SOON";
        }
    }

    private static bool TryGetLobbyInfo(out string info)
    {
        info = string.Empty;

        try
        {
            if (!AmongUsClient.Instance || !LobbyBehaviour.Instance || !GameData.Instance)
            {
                return false;
            }

            var code = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            info = $"Code: {code}";
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool _sceneHookAdded;
    private static bool _retrying;

    private static void InitializeDiscord(DiscordManager __instance)
    {
        try
        {
            ConnectToDiscord(__instance);
        }
        catch (Exception e)
        {
            // Discord is not running (yet), or the game cannot reach it. Keep trying in the background.
            Warning($"[Discord] Could not connect to Discord: {e.Message}");

            if (IsRunningUnderWine())
            {
                Warning("[Discord] This looks like Wine/Proton. Discord on Linux needs a bridge, see LPR_DATA/Linux-Discord.txt");
                WriteLinuxHelp();
            }

            if (!_retrying)
            {
                _retrying = true;
                Coroutines.Start(RetryConnect(__instance));
            }
        }
    }

    private static void ConnectToDiscord(DiscordManager manager)
    {
        var presence = new Discord.Discord(ClientId, (ulong)CreateFlags.NoRequireDiscord);
        manager.presence = presence;

        try
        {
            var activityManager = presence.GetActivityManager();

            activityManager.RegisterSteam(SteamAppId);
            activityManager.add_OnActivityJoin((Action<string>)manager.HandleJoinRequest);
            manager.SetInMenus();
        }
        catch
        {
            manager.presence = null;
            throw;
        }

        if (!_sceneHookAdded)
        {
            _sceneHookAdded = true;
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)((scene, _) =>
            {
                manager.OnSceneChange(scene.name);
            }));
        }
    }

    private static IEnumerator RetryConnect(DiscordManager manager)
    {
        while (manager && manager.presence == null)
        {
            yield return new WaitForSeconds(15f);

            if (!manager)
            {
                break;
            }

            try
            {
                ConnectToDiscord(manager);
                Info("[Discord] Connected to Discord");
            }
            catch
            {
                // Still no Discord, try again later.
            }
        }

        _retrying = false;
    }

    private static bool IsRunningUnderWine()
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WINEPREFIX"))
               || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STEAM_COMPAT_DATA_PATH"))
               || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROTON_VERSION"));
    }

    // Written next to the other LPR_DATA files so Linux players can find the steps.
    private static void WriteLinuxHelp()
    {
        try
        {
            var path = Path.Combine(Features.LprData.Root, "Linux-Discord.txt");
            if (File.Exists(path))
            {
                return;
            }

            Directory.CreateDirectory(Features.LprData.Root);
            File.WriteAllText(path,
                "Discord rich presence on Linux (Wine / Proton)\n" +
                "\n" +
                "The game talks to Discord over a Windows pipe, but Discord on Linux listens on a Unix socket.\n" +
                "wine-discord-ipc-bridge connects the two.\n" +
                "\n" +
                "1. Download winediscordipcbridge.exe from https://github.com/0e4ef622/wine-discord-ipc-bridge\n" +
                "2. Steam > Among Us > Properties > Launch Options (replace 1000 with the output of: id -u):\n" +
                "   PROTON_REMOTE_DEBUG_CMD=/path/to/winediscordipcbridge.exe PRESSURE_VESSEL_FILESYSTEMS_RW=\"/run/user/1000/discord-ipc-0\" %command%\n" +
                "3. Start Discord BEFORE starting the game.\n" +
                "\n" +
                "Flatpak Discord: the socket is not in the usual place. Link it first (see the Flathub Discord wiki):\n" +
                "   ln -sf $XDG_RUNTIME_DIR/app/com.discordapp.Discord/discord-ipc-0 $XDG_RUNTIME_DIR/discord-ipc-0\n");
        }
        catch (Exception e)
        {
            Warning($"[Discord] Could not write the Linux help file: {e.Message}");
        }
    }
}
