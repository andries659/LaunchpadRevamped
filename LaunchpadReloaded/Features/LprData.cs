using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Features;

/// <summary>
/// The LPR_DATA folder next to the game: the welcome message and the tag lists the host can edit.
/// <code>
/// LPR_DATA/
///   Welcome/welcome.txt
///   Tags/Mod-Tags/{tag.txt, players.txt}
///   Tags/Sponsor-Tags/{tag.txt, players.txt}
///   Tags/VIP-Tags/{tag.txt, players.txt}
/// </code>
/// </summary>
public static class LprData
{
    public sealed record TagStyle(string Text, Color[] Colors);

    // Earlier groups win when a player is listed in more than one.
    private static readonly (string Folder, string Text, string Color)[] TagGroups =
    [
        ("Mod-Tags", "MOD", "#3FA9F5"),
        ("Sponsor-Tags", "SPONSOR", "#F5B83F"),
        ("VIP-Tags", "VIP", "#57D957"),
    ];

    // Tags that are always there, whatever the files say. Friend code -> tag (several colours make a gradient).
    private static readonly Dictionary<string, TagStyle> BuiltInTags = new(StringComparer.OrdinalIgnoreCase)
    {
        ["teemothy#6171"] = new TagStyle("\u2605 \u2606 Mod Developer \u2606 \u2605", ParseColors("#120982, #7100B3, #850096, #B700AC, #CE0098")!),
    };

    private const string DefaultWelcome = "Welcome to the lobby, {{PlayerName}}!\nThis lobby is hosted by {{HostName}} ({{PlayerCount}} players, map: {{Map}}). Room code: {{RoomCode}}";

    public static string Root => Path.Combine(Paths.GameRootPath, "LPR_DATA");
    public static string WelcomeFolder => Path.Combine(Root, "Welcome");
    public static string WelcomeFile => Path.Combine(WelcomeFolder, "welcome.txt");
    private static string TagsRoot => Path.Combine(Root, "Tags");

    /// <summary>
    /// Creates the folders and example files. Existing files are never overwritten.
    /// </summary>
    public static void EnsureCreated()
    {
        try
        {
            // The folder used to be called LPR-DATA; keep anything already in it.
            var oldRoot = Path.Combine(Paths.GameRootPath, "LPR-DATA");
            if (Directory.Exists(oldRoot) && !Directory.Exists(Root))
            {
                Directory.Move(oldRoot, Root);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(WelcomeFile)!);
            WriteIfMissing(WelcomeFile, DefaultWelcome);

            foreach (var (folder, text, color) in TagGroups)
            {
                var dir = Path.Combine(TagsRoot, folder);
                Directory.CreateDirectory(dir);

                WriteIfMissing(Path.Combine(dir, "tag.txt"),
                    "// How the tag looks. Text is what is shown above the player.\n// Color is a hex code. Give several, separated by commas, for a gradient: #FF0000, #0000FF\n" +
                    $"Text={text}\nColor={color}\n");

                WriteIfMissing(Path.Combine(dir, "players.txt"),
                    "// One player per line, by friend code:  name#1234\n" +
                    "// Optionally give a player their own look:  name#1234 | Text | #RRGGBB   (or several colours: #RRGGBB, #RRGGBB)\n" +
                    "// Lines starting with // are ignored.\n");
            }

            WriteIfMissing(Path.Combine(Root, "README.txt"),
                "Welcome/*.txt   The messages every player gets in chat when they join the lobby (welcome.txt first, then the others by name). The host can edit them in the lobby with F6. Values like {{PlayerName}}, {{HostName}}, {{RoomCode}}, {{Map}}, {{PlayerCount}}, {{KillCooldown}} are filled in.\n" +
                "Tags/<group>/tag.txt     Text and Color of that group's tag.\n" +
                "Tags/<group>/players.txt Friend codes (name#1234) that get the tag.\n" +
                "Only the host's files are used. Changes are picked up the next time a player joins.\n");
        }
        catch (Exception e)
        {
            Logger<LaunchpadRevampedPlugin>.Warning($"Could not create LPR_DATA: {e.Message}");
        }
    }

    private static void WriteIfMissing(string path, string contents)
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, contents);
        }
    }

    /// <summary>
    /// welcome.txt always comes first, the other messages follow in name order.
    /// </summary>
    public static int WelcomeOrder(string name)
    {
        return string.Equals(name, "welcome", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
    }

    public static List<string> SortedWelcomeFiles()
    {
        if (!Directory.Exists(WelcomeFolder))
        {
            return [];
        }

        return Directory.GetFiles(WelcomeFolder, "*.txt")
            .OrderBy(path => WelcomeOrder(Path.GetFileNameWithoutExtension(path)))
            .ThenBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Every message in LPR_DATA/Welcome (every .txt file), in the order they are sent to a joining player.
    /// </summary>
    public static List<string> ReadWelcomeMessages()
    {
        var messages = new List<string>();

        foreach (var path in SortedWelcomeFiles())
        {
            try
            {
                var text = Unescape(File.ReadAllText(path).Replace("\r\n", "\n")).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    messages.Add(text);
                }
            }
            catch (Exception e)
            {
                Logger<LaunchpadRevampedPlugin>.Warning($"Could not read {path}: {e.Message}");
            }
        }

        return messages;
    }

    // A block starts with "name:" at the very start of a line (lowercase letters, digits, - or _) and runs until the
    // next such line. welcome.txt looks like:  welcome:...  rules:...  onmeeting:...  vip:...  help:...
    private static readonly Regex BlockHeader = new("^([a-z][a-z0-9_-]*):", RegexOptions.Compiled);

    /// <summary>
    /// Splits a welcome file into its named blocks (key = the name before the colon, case-insensitive).
    /// Everything inside a block is kept exactly as written (rich text tags, spaces, line breaks); only \n escapes are
    /// turned into real line breaks and blank lines around the block are dropped. Text before the first header is ignored.
    /// </summary>
    public static Dictionary<string, string> ParseWelcomeBlocks(string raw)
    {
        var blocks = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        List<string>? current = null;

        foreach (var line in raw.Replace("\r\n", "\n").Split('\n'))
        {
            var header = BlockHeader.Match(line);
            if (header.Success)
            {
                var name = header.Groups[1].Value;
                if (!blocks.TryGetValue(name, out current))
                {
                    current = blocks[name] = [];
                }

                var rest = line[header.Length..]; // text on the header line is the start of the block
                if (rest.Length > 0)
                {
                    current.Add(rest);
                }

                continue;
            }

            current?.Add(line);
        }

        return blocks.ToDictionary(
            pair => pair.Key,
            pair => Unescape(string.Join("\n", pair.Value)).Trim('\n').TrimEnd(),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The "onmeeting:" block of welcome.txt and nothing else, or null when there is none (or it is empty).
    /// </summary>
    public static string? ReadOnMeetingMessage()
    {
        try
        {
            if (!File.Exists(WelcomeFile))
            {
                return null;
            }

            var blocks = ParseWelcomeBlocks(File.ReadAllText(WelcomeFile));
            return blocks.TryGetValue("onmeeting", out var text) && !string.IsNullOrWhiteSpace(text) ? text : null;
        }
        catch (Exception e)
        {
            Logger<LaunchpadRevampedPlugin>.Warning($"Could not read {WelcomeFile}: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Turns typed escapes into the real characters: \n is a new line, \t a tab and \\ a single backslash.
    /// </summary>
    public static string Unescape(string text)
    {
        if (!text.Contains('\\'))
        {
            return text;
        }

        var result = new System.Text.StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                var next = text[i + 1];
                if (next is 'n' or 't' or '\\')
                {
                    result.Append(next == 'n' ? '\n' : next == 't' ? '\t' : '\\');
                    i++;
                    continue;
                }
            }

            result.Append(text[i]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Looks the friend code up in the tag lists. Returns null when the player has no tag.
    /// </summary>
    public static TagStyle? FindTag(string? friendCode)
    {
        if (string.IsNullOrWhiteSpace(friendCode))
        {
            return null;
        }

        var wanted = friendCode.Trim();

        if (BuiltInTags.TryGetValue(wanted, out var builtIn))
        {
            return builtIn;
        }

        foreach (var (folder, defaultText, defaultColor) in TagGroups)
        {
            try
            {
                var dir = Path.Combine(TagsRoot, folder);
                var playersFile = Path.Combine(dir, "players.txt");
                if (!File.Exists(playersFile))
                {
                    continue;
                }

                var settings = ReadSettings(Path.Combine(dir, "tag.txt"));
                var groupText = settings.GetValueOrDefault("Text", defaultText);
                var groupColor = ParseColors(settings.GetValueOrDefault("Color", defaultColor)) ?? ParseColors(defaultColor)!;

                foreach (var raw in File.ReadAllLines(playersFile))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith("//"))
                    {
                        continue;
                    }

                    var parts = line.Split('|').Select(part => part.Trim()).ToArray();
                    if (!string.Equals(parts[0], wanted, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var text = parts.Length > 1 && parts[1].Length > 0 ? parts[1] : groupText;
                    var colors = parts.Length > 2 ? ParseColors(parts[2]) ?? groupColor : groupColor;
                    return new TagStyle(text, colors);
                }
            }
            catch (Exception e)
            {
                Logger<LaunchpadRevampedPlugin>.Warning($"Could not read {folder}: {e.Message}");
            }
        }

        return null;
    }

    private static Dictionary<string, string> ReadSettings(string path)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
        {
            return result;
        }

        foreach (var raw in File.ReadAllLines(path))
        {
            var line = raw.Trim();
            var split = line.IndexOf('=');
            if (line.Length == 0 || line.StartsWith("//") || split <= 0)
            {
                continue;
            }

            result[line[..split].Trim()] = line[(split + 1)..].Trim();
        }

        return result;
    }

    public static Color? ParseColor(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        hex = hex.Trim();
        if (!hex.StartsWith('#'))
        {
            hex = "#" + hex;
        }

        return ColorUtility.TryParseHtmlString(hex, out var color) ? color : null;
    }

    /// <summary>
    /// Parses one hex colour or a comma separated list of them.
    /// </summary>
    public static Color[]? ParseColors(string? list)
    {
        if (string.IsNullOrWhiteSpace(list))
        {
            return null;
        }

        var colors = list.Split(',')
            .Select(part => ParseColor(part))
            .Where(color => color.HasValue)
            .Select(color => color!.Value)
            .ToArray();

        return colors.Length > 0 ? colors : null;
    }
}
