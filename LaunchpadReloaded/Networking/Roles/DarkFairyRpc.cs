using LaunchpadRevamped.Roles.Coven;
using Reactor.Networking.Attributes;

namespace LaunchpadRevamped.Networking.Roles;

public static class DarkFairyRpc
{
    /// <summary>
    /// Adds a task to the marked set (sent by the Dark Fairy) or removes it (sent by the player it just darkened).
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.DarkFairyMark)]
    public static void RpcDarkFairyMark(this PlayerControl sender, int taskIndex, bool add)
    {
        if (add)
        {
            if (sender.Data.Role is not DarkFairyRole)
            {
                sender.KickForCheating();
                return;
            }

            DarkFairyRole.Marked.Add(taskIndex);
        }
        else
        {
            DarkFairyRole.Marked.Remove(taskIndex);
        }
    }
}
