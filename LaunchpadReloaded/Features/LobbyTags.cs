using System.Collections.Generic;
using System.Linq;
using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options;
using MiraAPI.GameOptions;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace LaunchpadRevamped.Features;

/// <summary>
/// One tag for one player, as sent by the host to everyone in the lobby. Hex is one colour or a comma separated gradient.
/// </summary>
public readonly record struct LobbyTagEntry(byte PlayerId, string Text, string Hex);

/// <summary>
/// The tags players currently have. The host builds them from LPR_DATA/Tags and syncs them to every client.
/// </summary>
public static class LobbyTags
{
    private static readonly Dictionary<byte, LobbyTagEntry> Entries = [];

    public static void Set(IEnumerable<LobbyTagEntry> entries)
    {
        Entries.Clear();
        foreach (var entry in entries)
        {
            Entries[entry.PlayerId] = entry;
        }
    }

    public static bool TryGet(byte playerId, out LobbyTagEntry entry)
    {
        return Entries.TryGetValue(playerId, out entry);
    }

    /// <summary>
    /// Host only: reads the tag files, applies them locally and sends them to everyone.
    /// </summary>
    public static void Broadcast()
    {
        if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost || !PlayerControl.LocalPlayer)
        {
            return;
        }

        var list = new List<LobbyTagEntry>();

        var enabled = true;
        try
        {
            enabled = OptionGroupSingleton<GeneralOptions>.Instance.PlayerTags;
        }
        catch
        {
            // Options are not ready, treat tags as on.
        }

        if (enabled)
        {
            foreach (var client in AmongUsClient.Instance.allClients)
            {
                if (!client.Character)
                {
                    continue;
                }

                var style = LprData.FindTag(client.FriendCode);
                if (style is null)
                {
                    continue;
                }

                list.Add(new LobbyTagEntry(client.Character.PlayerId, style.Text, string.Join(",", style.Colors.Select(color => "#" + ColorUtility.ToHtmlStringRGB(color)))));
            }
        }

        Set(list);
        Rpc<RpcSyncLobbyTags>.Instance.Send(PlayerControl.LocalPlayer, list);
    }
}
