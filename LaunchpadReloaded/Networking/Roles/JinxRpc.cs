using LaunchpadRevamped.Roles.Coven;
using Reactor.Networking.Attributes;

namespace LaunchpadRevamped.Networking.Roles;

public static class JinxRpc
{
    /// <summary>
    /// Adds (the Jinx casting) or removes (the host, once the jinx has paid out) a jinx on a player.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.JinxMark)]
    public static void RpcJinx(this PlayerControl jinx, byte targetId, bool add)
    {
        if (jinx.Data.Role is not JinxRole)
        {
            if (add)
            {
                jinx.KickForCheating();
            }

            return;
        }

        if (add)
        {
            JinxRole.Jinxed[targetId] = jinx.PlayerId;
        }
        else
        {
            JinxRole.Jinxed.Remove(targetId);
        }
    }
}
