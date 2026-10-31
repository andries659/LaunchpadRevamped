using System.Collections;
using System.Linq;
using AmongUs.GameOptions;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Networking.Roles;

public static class CovenRpc
{
    /// <summary>
    /// Sent on behalf of the frozen player. Every client runs it, but only the frozen player's own client holds them in place.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.CovenFreeze)]
    public static void RpcFreeze(this PlayerControl target, float duration)
    {
        if (!target.AmOwner || target.Data.IsDead)
        {
            return;
        }

        Coroutines.Start(CoFreeze(target, Mathf.Clamp(duration, 0.5f, 15f)));
    }

    private static IEnumerator CoFreeze(PlayerControl player, float duration)
    {
        var elapsed = 0f;
        while (elapsed < duration && player && !player.Data.IsDead && !MeetingHud.Instance)
        {
            elapsed += Time.deltaTime;
            player.moveable = false;
            player.MyPhysics.body.velocity = Vector2.zero;
            yield return null;
        }

        if (player && !MeetingHud.Instance && !Minigame.Instance)
        {
            player.moveable = true;
        }
    }

    /// <summary>
    /// Brings a dead body back to life as a plain Crewmate.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.CovenReanimate)]
    public static void RpcReanimate(this PlayerControl necromancer, byte bodyId)
    {
        if (necromancer.Data.Role is not NecromancerRole)
        {
            necromancer.KickForCheating();
            return;
        }

        var body = MiraAPI.Utilities.Helpers.GetBodyById(bodyId);
        if (body == null || !body)
        {
            return;
        }

        var revived = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player.PlayerId == body.ParentId);

        body.Revive(necromancer);

        if (revived)
        {
            RoleManager.Instance.SetRole(revived, RoleTypes.Crewmate);
        }
    }
}
