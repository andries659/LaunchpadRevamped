// Test bots: fake players you can add to a local lobby to test roles on your own.
// Switching control between players is adapted from Night-GUA/AmongUs.MultiClientInstancing (GPL-3.0), originally by
// MyDragonBreath and whichTwix. Bot spawning follows bcdidit67/AmongUs-AdvancedBot (MIT): fake ClientData players
// are dropped by the server as soon as the game starts, so the bots are "dummy" players (GameData.AddDummy) instead.
// They are registered with GameData, so they get a role at game start like any player, and you can take control of
// any of them. Only meant for local (LAN/offline) lobbies with no other humans.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LaunchpadRevamped.Features.TestClients;

public static class TestClientManager
{
    private const string BotName = "Bot";

    /// <summary>True only inside a local-game lobby/game. Everything below is a no-op otherwise.</summary>
    public static bool Enabled { get; set; }

    /// <summary>Re-create the bots when the lobby reloads after a game.</summary>
    public static bool KeepBots { get; set; } = true;

    private static readonly HashSet<byte> BotIds = [];
    private static int _botsToRestore;
    private static byte _realPlayerId;

    public static bool HasBots => BotIds.Count > 0;

    public static bool IsBot(byte playerId) => BotIds.Contains(playerId);

    public static PlayerControl? PlayerById(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p && p.PlayerId == id);
    }

    public static void CreateBot()
    {
        var client = AmongUsClient.Instance;
        var gameData = GameData.Instance;
        if (!client || !gameData || !client.PlayerPrefab)
        {
            Warning("Can't add a bot yet: client or GameData isn't ready.");
            return;
        }

        if (!BotIds.Any() && PlayerControl.LocalPlayer)
        {
            _realPlayerId = PlayerControl.LocalPlayer.PlayerId;
        }

        var bot = Object.Instantiate(client.PlayerPrefab);
        bot.isDummy = true;
        bot.PlayerId = (byte)gameData.GetAvailableId();

        // DummyBehaviour makes a dummy trail the local player around; bots should stand still.
        var follow = bot.GetComponent<DummyBehaviour>();
        if (follow)
        {
            follow.enabled = false;
            Object.Destroy(follow);
        }

        var name = $"{BotName} {bot.PlayerId}";
        bot.SetName(name);
        bot.SetColor(Random.Range(0, Palette.PlayerColors.Length));

        var info = gameData.AddDummy(bot);
        if (info != null)
        {
            info.PlayerName = name;
            info.MarkDirty();
        }

        client.Spawn(bot, client.ClientId, SpawnFlags.None);

        // Dummies skip the normal lobby spawn, so seat them by hand.
        var seats = LobbyBehaviour.Instance ? LobbyBehaviour.Instance.SpawnPositions : null;
        if (seats is { Length: > 0 })
        {
            var seat = seats[BotIds.Count % seats.Length];
            var position = bot.transform.position;
            bot.transform.position = new Vector3(seat.x, seat.y, position.z);
        }

        bot.moveable = false;
        BotIds.Add(bot.PlayerId);

        Info($"Added bot {bot.PlayerId}.");
    }

    public static void RemoveBot(byte playerId)
    {
        if (!BotIds.Remove(playerId))
        {
            return;
        }

        var bot = PlayerById(playerId);
        try
        {
            if (bot)
            {
                AmongUsClient.Instance.Despawn(bot);
            }

            GameData.Instance.RemovePlayer(playerId);
        }
        catch (System.Exception e)
        {
            Warning($"Couldn't cleanly remove bot {playerId}: {e.Message}");
        }
    }

    public static void RemoveAllBots()
    {
        // Hand control back to the real player before their bots disappear.
        SwitchTo(_realPlayerId);

        foreach (var id in BotIds.ToList())
        {
            RemoveBot(id);
        }
    }

    /// <summary>Called when a lobby loads: the old bots are gone with the old scene, so rebuild them if KeepBots is on.</summary>
    public static void OnLobbyLoaded()
    {
        _botsToRestore = BotIds.Count > 0 ? BotIds.Count : _botsToRestore;
        BotIds.Clear();

        if (!KeepBots || _botsToRestore == 0)
        {
            return;
        }

        Coroutines.Start(CoRestoreBots(_botsToRestore));
    }

    private static IEnumerator CoRestoreBots(int count)
    {
        // Give the lobby a moment to finish setting up GameData and the seats.
        for (var frame = 0; frame < 30; frame++)
        {
            yield return null;
        }

        if (!LobbyBehaviour.Instance)
        {
            yield break;
        }

        for (var i = 0; i < count; i++)
        {
            CreateBot();
            yield return null;
        }
    }

    /// <summary>Move forwards (+1) or backwards (-1) through every player, wrapping around.</summary>
    public static void SwitchRelative(int step)
    {
        var players = PlayerControl.AllPlayerControls.ToArray().Where(p => p).OrderBy(p => p.PlayerId).ToList();
        if (players.Count == 0)
        {
            return;
        }

        var current = players.FindIndex(p => p.PlayerId == PlayerControl.LocalPlayer.PlayerId);
        var next = (current + step + players.Count) % players.Count;
        SwitchTo(players[next].PlayerId);
    }

    public static void SwitchTo(byte playerId)
    {
        var newPlayer = PlayerById(playerId);
        var old = PlayerControl.LocalPlayer;
        if (!newPlayer || !old || newPlayer == old)
        {
            return;
        }

        old.NetTransform.RpcSnapTo(old.transform.position);
        old.moveable = false;

        var light = old.lightSource;

        PlayerControl.LocalPlayer = newPlayer;
        newPlayer.lightSource = light;
        newPlayer.moveable = true;

        AmongUsClient.Instance.ClientId = newPlayer.OwnerId;
        AmongUsClient.Instance.HostId = newPlayer.OwnerId;

        HudManager.Instance.SetHudActive(true);

        light.transform.SetParent(newPlayer.transform);
        light.transform.localPosition = newPlayer.Collider.offset;
        Camera.main!.GetComponent<FollowerCamera>().SetTarget(newPlayer);

        newPlayer.MyPhysics.ResetMoveState(true);
        KillAnimation.SetMovement(newPlayer, true);
        newPlayer.MyPhysics.inputHandler.enabled = true;

        Info($"Now controlling player {newPlayer.PlayerId} ({newPlayer.Data.PlayerName}).");
    }
}
