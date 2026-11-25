using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BepInEx;
using InnerNet;
using UnityEngine;
using System.Reflection;

namespace LaunchpadRevamped.Features;

public static class ChatCommands
{
    public const string Prefix = "/t";

    private static string FilePath => Path.Combine(Paths.GameRootPath, "LPR_DATA", "Welcome", "welcome.txt");

    private const string DefaultFile =
        "# One command per line:  name:text\n" +
        "# Use {{PlayerName}}, {{RoomCode}}, {{PlayerCount}} etc. for live values, and \\n for a line break.\n" +
        "welcome:Welcome {{PlayerName}}!\\nRoom {{RoomCode}} hosted by {{HostName}}\n" +
        "rules:Lobby rules here\n";

    // ---- game stats (fed by GameStatsPatches) ----
    private static int _meetingCount;
    private static DateTime? _gameStart;
    private static DateTime? _gameEnd;

    public static void GameStarted()
    {
        _meetingCount = 0;
        _gameStart = DateTime.Now;
        _gameEnd = null;
    }

    public static void MeetingStarted() => _meetingCount++;

    // Keeps the last game's numbers visible in the lobby until the next game starts.
    public static void BackInLobby()
    {
        if (_gameStart != null && _gameEnd == null)
        {
            _gameEnd = DateTime.Now;
        }
    }

    // ---- placeholders ----
    private static readonly string[] MapNames = { "The Skeld", "MIRA HQ", "Polus", "Dleks", "Airship", "The Fungle" };

    private static readonly Dictionary<string, Func<string>> Placeholders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PlayerName"] = () => PlayerControl.LocalPlayer.Data.PlayerName,
        ["ModVersion"] = ModVersion,
        ["HostName"] = () => AmongUsClient.Instance.GetHost()?.PlayerName ?? "Unknown",
        ["RoomCode"] = () => GameCode.IntToGameName(AmongUsClient.Instance.GameId),
        ["AmongUsVersion"] = () => Application.version,
        ["InternalVersion"] = () => Constants.GetBroadcastVersion().ToString(),

        ["Players"] = () => Names(Connected()),
        ["AlivePlayers"] = () => Names(Connected().Where(p => !p.IsDead)),
        ["DeadPlayers"] = () => Names(Connected().Where(p => p.IsDead)),
        ["PlayerCount"] = () => Connected().Count.ToString(),
        ["AlivePlayerCount"] = () => Connected().Count(p => !p.IsDead).ToString(),
        ["DeadPlayerCount"] = () => Connected().Count(p => p.IsDead).ToString(),

        ["Map"] = () =>
        {
            var id = GameOptionsManager.Instance.CurrentGameOptions.MapId;
            return id < MapNames.Length ? MapNames[id] : $"Map {id}";
        },
        ["KillCooldown"] = () => F(GameOptionsManager.Instance.currentNormalGameOptions.KillCooldown) + "s",
        ["DiscussionTime"] = () => GameOptionsManager.Instance.currentNormalGameOptions.DiscussionTime + "s",
        ["VotingTime"] = () => GameOptionsManager.Instance.currentNormalGameOptions.VotingTime + "s",
        ["EmergencyCooldown"] = () => GameOptionsManager.Instance.currentNormalGameOptions.EmergencyCooldown + "s",
        ["NumEmergencyMeetings"] = () => GameOptionsManager.Instance.currentNormalGameOptions.NumEmergencyMeetings.ToString(),
        ["PlayerSpeedMod"] = () => F(GameOptionsManager.Instance.currentNormalGameOptions.PlayerSpeedMod) + "x",
        ["CrewLightMod"] = () => F(GameOptionsManager.Instance.currentNormalGameOptions.CrewLightMod) + "x",
        ["ImpostorLightMod"] = () => F(GameOptionsManager.Instance.currentNormalGameOptions.ImpostorLightMod) + "x",
        ["NumCommonTasks"] = () => GameOptionsManager.Instance.currentNormalGameOptions.NumCommonTasks.ToString(),
        ["NumLongTasks"] = () => GameOptionsManager.Instance.currentNormalGameOptions.NumLongTasks.ToString(),
        ["NumShortTasks"] = () => GameOptionsManager.Instance.currentNormalGameOptions.NumShortTasks.ToString(),

        ["MeetingCount"] = () => _meetingCount.ToString(),
        ["GameDuration"] = () =>
        {
            if (_gameStart == null)
            {
                return "0:00";
            }

            var d = (_gameEnd ?? DateTime.Now) - _gameStart.Value;
            return $"{(int)d.TotalMinutes}:{d.Seconds:00}";
        },
        ["Date"] = () => DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        ["Time"] = () => DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture),

        // TODO: point this at wherever your mod stores the selected preset.
        ["Preset"] = () => "None"
    };

    private static readonly Regex PlaceholderRegex = new(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled);

    private static string Apply(string line)
    {
        return PlaceholderRegex.Replace(line, match =>
        {
            if (!Placeholders.TryGetValue(match.Groups[1].Value, out var get))
            {
                return match.Value; // unknown placeholder: leave it as typed
            }

            try
            {
                return get() ?? "";
            }
            catch
            {
                return "N/A"; // e.g. no lobby/game data available right now
            }
        });
    }

    private static string F(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static List<NetworkedPlayerInfo> Connected()
    {
        var list = new List<NetworkedPlayerInfo>();

        foreach (var player in GameData.Instance.AllPlayers)
        {
            if (player != null && !player.Disconnected)
            {
                list.Add(player);
            }
        }

        return list;
    }

    private static string Names(IEnumerable<NetworkedPlayerInfo> players)
    {
        var names = players.Select(p => p.PlayerName).ToArray();
        return names.Length == 0 ? "None" : string.Join(", ", names);
    }

    // ---- commands ----

    /// <summary>Returns the lines to show, or null if this isn't a command.</summary>
    public static string[]? Run(string input)
    {
        input = input.Trim();

        // /r lists the enabled and disabled roles.
        if (input.Equals("/r", StringComparison.OrdinalIgnoreCase))
        {
            return RoleListCommand.Build();
        }

        if (!input.StartsWith(Prefix + " ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var name = input[Prefix.Length..].Trim();
        if (name.Length == 0)
        {
            return null;
        }

        // Read on every use so edits to the file apply immediately.
        if (ReadFile().TryGetValue(name, out var lines))
        {
            return lines.Select(Apply).ToArray();
        }

        return new[] { $"Unknown command '{name}'." };
    }

    // Turns the two characters "\n" in the file into a real line break, and so on.
    private static string Unescape(string text)
    {
        if (!text.Contains('\\'))
        {
            return text;
        }

        var sb = new StringBuilder(text.Length);

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                switch (text[i + 1])
                {
                    case 'n': sb.Append('\n'); i++; continue;
                    case 't': sb.Append("    "); i++; continue;
                    case '\\': sb.Append('\\'); i++; continue;
                }
            }

            sb.Append(text[i]);
        }

        return sb.ToString();
    }

private static string ModVersion()
{
    var assembly = typeof(ChatCommands).Assembly;

    var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                  ?? assembly.GetName().Version?.ToString(3)
                  ?? "?";

    // Drop a "+commit" tail if one is ever added to the version.
    var plus = version.IndexOf('+');
    if (plus >= 0)
    {
        version = version[..plus];
    }

    return $"Launchpad: Reloaded v{version}";
}

    private static Dictionary<string, List<string>> ReadFile()
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        try
        {
            if (!File.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, DefaultFile);
            }

            string? current = null;

            foreach (var raw in File.ReadAllLines(FilePath))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }

                var colon = line.IndexOf(':');
                var key = colon > 0 ? line[..colon].Trim() : "";

                // "name:text" starts/extends a command. The name can't contain spaces,
                // so a line like "Rule 1: be nice" is treated as more text for the previous command.
                if (key.Length > 0 && !key.Contains(' '))
                {
                    current = key;
                    var text = Unescape(line[(colon + 1)..].Trim());

                    if (!result.ContainsKey(current))
                    {
                        result[current] = new List<string>();
                    }

                    if (text.Length > 0)
                    {
                        result[current].Add(text);
                    }
                }
                else if (current != null)
                {
                    result[current].Add(Unescape(line));
                }
            }
        }
        catch
        {
            // unreadable file: fall through with whatever was parsed
        }

        return result;
    }
}
