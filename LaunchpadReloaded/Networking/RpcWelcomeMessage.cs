using System.Collections.Generic;
using System.Linq;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace LaunchpadRevamped.Networking;

/// <summary>
/// One piece of a welcome message. A long message is cut into pieces so it fits in the network packets.
/// </summary>
public readonly record struct WelcomeChunk(byte Index, byte Total, string Text);

/// <summary>
/// Sent by the host to a player who just joined the lobby. The player shows it in their chat as a system message.
/// </summary>
[RegisterCustomRpc((uint)LaunchpadRpc.WelcomeMessage)]
public class RpcWelcomeMessage(LaunchpadRevampedPlugin plugin, uint id) : PlayerCustomRpc<LaunchpadRevampedPlugin, WelcomeChunk>(plugin, id)
{
    private const int ChunkLength = 250;
    private const string SenderName = "<color=#FFD54A>SYSTEM MESSAGE</color>";

    private static string?[] _pieces = [];

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    /// <summary>
    /// Cuts a message into pieces that each fit in one packet.
    /// </summary>
    public static List<WelcomeChunk> Split(string message)
    {
        var parts = new List<string>();
        var start = 0;

        while (start < message.Length)
        {
            var length = System.Math.Min(ChunkLength, message.Length - start);

            // Never cut an emoji (a surrogate pair) in half.
            if (start + length < message.Length && char.IsHighSurrogate(message[start + length - 1]))
            {
                length--;
            }

            parts.Add(message.Substring(start, length));
            start += length;
        }

        var total = (byte)System.Math.Min(parts.Count, byte.MaxValue);
        return parts.Take(total).Select((text, index) => new WelcomeChunk((byte)index, total, text)).ToList();
    }

    public override void Write(MessageWriter writer, WelcomeChunk data)
    {
        writer.Write(data.Index);
        writer.Write(data.Total);
        writer.Write(data.Text);
    }

    public override WelcomeChunk Read(MessageReader reader)
    {
        return new WelcomeChunk(reader.ReadByte(), reader.ReadByte(), reader.ReadString());
    }

    public override void Handle(PlayerControl pc, WelcomeChunk data)
    {
        // Only the host is allowed to send this.
        if (AmongUsClient.Instance.HostId != pc.OwnerId || data.Total == 0 || data.Index >= data.Total)
        {
            return;
        }

        if (data.Index == 0 || _pieces.Length != data.Total)
        {
            _pieces = new string?[data.Total];
        }

        _pieces[data.Index] = data.Text;

        if (_pieces.Any(piece => piece is null))
        {
            return;
        }

        var message = string.Concat(_pieces);
        _pieces = [];

        if (!string.IsNullOrWhiteSpace(message))
        {
            Show(pc, message);
        }
    }

    /// <summary>
    /// Host only: sends a message to every player and shows it in the host's own chat as the system message.
    /// </summary>
    public static void Broadcast(string message)
    {
        var host = PlayerControl.LocalPlayer;
        if (!host || !AmongUsClient.Instance.AmHost || string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        foreach (var chunk in Split(message))
        {
            Rpc<RpcWelcomeMessage>.Instance.Send(host, chunk);
        }

        Show(host, message); // LocalHandling is None, so the host adds it to its own chat here
    }

    private static void Show(PlayerControl host, string message)
    {
        if (!HudManager.Instance || !HudManager.Instance.Chat)
        {
            return;
        }

        var chat = HudManager.Instance.Chat;
        chat.AddChat(host, message);

        // The newest chat bubble is the one just added. Its header normally shows the host's name.
        try
        {
            var inner = chat.scroller.Inner;
            if (inner.childCount > 0)
            {
                var bubble = inner.GetChild(inner.childCount - 1).GetComponent<ChatBubble>();
                if (bubble)
                {
                    bubble.NameText.text = SenderName;
                }
            }
        }
        catch (System.Exception e)
        {
            Warning($"Could not rename the welcome message sender: {e.Message}");
        }
    }
}
