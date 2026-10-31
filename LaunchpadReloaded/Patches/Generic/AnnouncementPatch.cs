using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AmongUs.Data;
using AmongUs.Data.Player;
using Assets.InnerNet;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Reactor.Utilities;
using UnityEngine.Networking;

namespace LaunchpadRevamped.Patches.Generic;

[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class LaunchpadAnnouncementFetcherPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        LaunchpadAnnouncementFetcher.CheckForNews();
    }
}

[HarmonyPatch(typeof(PlayerAnnouncementData), nameof(PlayerAnnouncementData.SetAnnouncements))]
public static class LaunchpadAnnouncementHistoryPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref Il2CppReferenceArray<Announcement> aRange)
    {
        if (LaunchpadAnnouncementFetcher.News.Count == 0 || aRange == null)
        {
            return;
        }

        var announcements = aRange.ToArray().ToList();
        announcements.AddRange(LaunchpadAnnouncementFetcher.News
            .Select(news => news.ToAnnouncement())
            .Where(news => announcements.All(existing => existing.Number != news.Number)));

        announcements.Sort((left, right) => ParseDate(right.Date).CompareTo(ParseDate(left.Date)));
        aRange = new Il2CppReferenceArray<Announcement>(announcements.ToArray());
    }

    private static DateTimeOffset ParseDate(string? date)
    {
        return DateTimeOffset.TryParse(date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : DateTimeOffset.MinValue;
    }
}

internal static class LaunchpadAnnouncementFetcher
{
    private const string NewsUrl = "https://raw.githubusercontent.com/andries659/LaunchpadReloaded/refs/heads/master/LaunchpadReloaded/Resources/Announcements/modNews-en_US.json";
    private const string EmbeddedNewsName = "LaunchpadRevamped.Resources.Announcements.modNews-en_US.json";
    private static bool _isFetching;
    private static bool _hasFetched;

    internal static IReadOnlyList<LaunchpadModNews> News { get; private set; } = Array.Empty<LaunchpadModNews>();

    internal static void CheckForNews()
    {
        if (!_hasFetched && !_isFetching)
        {
            Coroutines.Start(FetchNews());
        }
    }

    private static IEnumerator FetchNews()
    {
        _isFetching = true;
        var request = UnityWebRequest.Get(NewsUrl);
        request.timeout = 10;
        yield return request.SendWebRequest();

        var succeeded = request.result == UnityWebRequest.Result.Success;
        var responseText = succeeded ? request.downloadHandler.text : string.Empty;
        var error = request.error;
        request.Dispose();

        if (succeeded && TryParseNews(responseText, out var news))
        {
            News = news;
            Info($"Loaded {News.Count} Launchpad announcement(s) from the news file.");
        }
        else
        {
            Warning($"Could not load remote Launchpad news ({error}); using the embedded news file.");
            LoadEmbeddedNews();
        }

        _hasFetched = true;
        _isFetching = false;
    }

    private static void LoadEmbeddedNews()
    {
        try
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(EmbeddedNewsName);
            if (stream == null)
            {
                Warning($"Embedded Launchpad news file was not found: {EmbeddedNewsName}");
                News = Array.Empty<LaunchpadModNews>();
                return;
            }

            using var reader = new StreamReader(stream);
            if (TryParseNews(reader.ReadToEnd(), out var news))
            {
                News = news;
                Info($"Loaded {News.Count} Launchpad announcement(s) from the embedded news file.");
            }
            else
            {
                Warning("The embedded Launchpad news file has an invalid format.");
                News = Array.Empty<LaunchpadModNews>();
            }
        }
        catch (Exception exception)
        {
            Warning($"Could not load the embedded Launchpad news file: {exception.Message}");
            News = Array.Empty<LaunchpadModNews>();
        }
    }

    private static bool TryParseNews(string jsonText, out List<LaunchpadModNews> news)
    {
        news = new List<LaunchpadModNews>();

        try
        {
            using var json = JsonDocument.Parse(jsonText);
            if (!json.RootElement.TryGetProperty("News", out var newsArray) || newsArray.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (var item in newsArray.EnumerateArray())
            {
                if (!TryGetNumber(item, out var number))
                {
                    continue;
                }

                var date = GetString(item, "Date");
                var shortTitle = GetString(item, "ShortTitle");
                var subTitle = GetString(item, "SubTitle");
                var title = GetString(item, "Title");
                var text = GetText(item);

                if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                news.Add(new LaunchpadModNews(number, title, subTitle, shortTitle, text, date));
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool TryGetNumber(JsonElement item, out int number)
    {
        number = 0;
        if (!item.TryGetProperty("Number", out var value))
        {
            return false;
        }

        if (value.ValueKind == JsonValueKind.Number)
        {
            return value.TryGetInt32(out number);
        }

        return value.ValueKind == JsonValueKind.String
            && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number);
    }

    private static string GetString(JsonElement item, string propertyName)
    {
        return item.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string GetText(JsonElement item)
    {
        if (!item.TryGetProperty("Text", out var text))
        {
            return string.Empty;
        }

        if (text.ValueKind == JsonValueKind.String)
        {
            return text.GetString() ?? string.Empty;
        }

        if (text.ValueKind == JsonValueKind.Array)
        {
            return string.Concat(text.EnumerateArray()
                .Where(line => line.ValueKind == JsonValueKind.String)
                .Select(line => line.GetString() ?? string.Empty));
        }

        return string.Empty;
    }
}

internal sealed record LaunchpadModNews(int Number, string Title, string SubTitle, string ShortTitle, string Text, string Date)
{
    internal Announcement ToAnnouncement()
    {
        return new Announcement
        {
            Date = Date,
            Number = Number,
            ShortTitle = ShortTitle,
            SubTitle = SubTitle,
            Title = Title,
            Text = Text,
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Id = "LaunchpadRevamped"
        };
    }
}
