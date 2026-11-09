using System.Collections.Generic;
using Hazel;
using LaunchpadRevamped.Features;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LaunchpadRevamped.Networking;

/// <summary>
/// Sent by the host to everyone: which players have which tag.
/// </summary>
[RegisterCustomRpc((uint)LaunchpadRpc.LobbyTags)]
public class RpcSyncLobbyTags(LaunchpadRevampedPlugin plugin, uint id) : PlayerCustomRpc<LaunchpadRevampedPlugin, List<LobbyTagEntry>>(plugin, id)
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, List<LobbyTagEntry>? data)
    {
        data ??= [];
        writer.Write((byte)data.Count);

        foreach (var entry in data)
        {
            writer.Write(entry.PlayerId);
            writer.Write(entry.Text);
            writer.Write(entry.Hex);
        }
    }

    public override List<LobbyTagEntry> Read(MessageReader reader)
    {
        var count = reader.ReadByte();
        var list = new List<LobbyTagEntry>(count);

        for (var i = 0; i < count; i++)
        {
            list.Add(new LobbyTagEntry(reader.ReadByte(), reader.ReadString(), reader.ReadString()));
        }

        return list;
    }

    public override void Handle(PlayerControl pc, List<LobbyTagEntry>? data)
    {
        // Only the host decides who has a tag.
        if (AmongUsClient.Instance.HostId != pc.OwnerId)
        {
            return;
        }

        LobbyTags.Set(data ?? []);
    }
}
