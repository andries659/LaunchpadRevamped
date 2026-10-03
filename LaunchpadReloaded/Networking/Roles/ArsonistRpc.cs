using System.Collections;
using System.Linq;
using LaunchpadRevamped.GameOver;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameEnd;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Networking.Roles;

public static class ArsonistRpc
{
    private static IEnumerator CoCheckWin(PlayerControl arsonist)
    {
        // Give the burn kills a moment to land before checking who is left.
        yield return new WaitForSeconds(0.5f);

        if (!arsonist || arsonist.Data.IsDead)
        {
            yield break;
        }

        var othersAlive = PlayerControl.AllPlayerControls.ToArray()
            .Any(player => player != arsonist && !player.Data.IsDead && !player.Data.Disconnected);

        if (!othersAlive)
        {
            CustomGameOver.Trigger<ArsonistGameOver>([arsonist.Data]);
        }
    }

    [MethodRpc((uint)LaunchpadRpc.ArsonistCheckWin)]
    public static void RpcCheckArsonistWin(this PlayerControl arsonist)
    {
        if (arsonist.Data.Role is not ArsonistRole)
        {
            arsonist.KickForCheating();
            return;
        }

        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        Coroutines.Start(CoCheckWin(arsonist));
    }
}
