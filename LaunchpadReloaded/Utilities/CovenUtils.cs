using System;
using System.Linq;
using LaunchpadRevamped.GameOver;
using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameEnd;
using UnityEngine;

namespace LaunchpadRevamped.Utilities;

public static class CovenUtils
{
    public static bool IsCoven(PlayerControl? player)
    {
        return player && player!.Data && player.Data.Role is ICovenRole;
    }

    /// <summary>
    /// The closest living player within range that is not part of the Coven, optionally narrowed by <paramref name="filter"/>.
    /// </summary>
    public static PlayerControl? ClosestTarget(float range, Func<PlayerControl, bool>? filter = null)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local)
        {
            return null;
        }

        return PlayerControl.AllPlayerControls.ToArray()
            .Where(player => player && !player.AmOwner && player.Data && !player.Data.IsDead && !player.Data.Disconnected
                             && !player.inVent && !IsCoven(player) && (filter == null || filter(player)))
            .Where(player => Vector2.Distance(local.GetTruePosition(), player.GetTruePosition()) <= range)
            .OrderBy(player => Vector2.Distance(local.GetTruePosition(), player.GetTruePosition()))
            .FirstOrDefault();
    }

    /// <summary>
    /// Host-side check: the Coven wins once it is at least as large as everyone else still alive.
    /// Never triggers before the first death, so small or test lobbies don't end instantly.
    /// </summary>
    public static bool TryTriggerCovenWin()
    {
        var players = PlayerControl.AllPlayerControls.ToArray()
            .Where(player => player && player.Data && !player.Data.Disconnected)
            .ToArray();

        if (!players.Any(player => player.Data.IsDead))
        {
            return false;
        }

        var alive = players.Where(player => !player.Data.IsDead).ToArray();
        var coven = alive.Where(player => IsCoven(player)).ToArray();
        if (coven.Length == 0 || alive.Length - coven.Length > coven.Length)
        {
            return false;
        }

        CustomGameOver.Trigger<CovenGameOver>(coven.Select(player => player.Data).ToArray());
        return true;
    }
}
