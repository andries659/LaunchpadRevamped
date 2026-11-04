using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Impostor;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LaunchpadRevamped.Networking.Roles;

public static class EsperRpc
{
    [MethodRpc((uint)LaunchpadRpc.EsperProject)]
    public static void RpcEsperProject(this PlayerControl playerControl, float x, float y)
    {
        if (playerControl.Data.Role is not EsperRole)
        {
            playerControl.KickForCheating();
            return;
        }

        EsperProjection.Begin(playerControl, new Vector2(x, y));
    }

    [MethodRpc((uint)LaunchpadRpc.EsperReturn)]
    public static void RpcEsperReturn(this PlayerControl playerControl)
    {
        if (playerControl.Data.Role is not EsperRole)
        {
            playerControl.KickForCheating();
            return;
        }

        EsperProjection.End(playerControl, snapToBody: true);
    }
}
