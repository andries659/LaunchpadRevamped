using LaunchpadRevamped.Roles.Impostor;
using Reactor.Networking.Attributes;

namespace LaunchpadRevamped.Networking.Roles;

public static class BlackmailerRpc
{
    /// <summary>
    /// Marks a player as blackmailed until the next meeting ends. A Blackmailer only holds one target at a time.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.Blackmail)]
    public static void RpcBlackmail(this PlayerControl blackmailer, byte targetId)
    {
        if (blackmailer.Data.Role is not BlackmailerRole)
        {
            blackmailer.KickForCheating();
            return;
        }

        BlackmailerRole.Blackmailed[blackmailer.PlayerId] = targetId;
    }
}
