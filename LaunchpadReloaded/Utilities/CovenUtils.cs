using System;
using System.Linq;
using LaunchpadRevamped.GameOver;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
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

        // Players darkened by the Dark Fairy are counted the way the Dark Fairy's setting says:
        // out of the game, on the Coven's side, or still on their original side.
        var darkened = alive.Where(player => player.HasModifier<DarkenedModifier>()).ToArray();
        var mode = OptionGroupSingleton<DarkFairyOptions>.Instance.DarkenedCountMode;
        var counted = mode == DarkFairyOptions.DarkenedCounts.None ? alive.Except(darkened).ToArray() : alive;
        var coven = counted
            .Where(player => IsCoven(player) || (mode == DarkFairyOptions.DarkenedCounts.DarkFairy && darkened.Contains(player)))
            .ToArray();

        if (!counted.Any(player => IsCoven(player)) || counted.Length - coven.Length > coven.Length)
        {
            return false;
        }

        // Every darkened player who is still alive wins together with the Coven.
        var winners = coven.Concat(darkened).Distinct().ToArray();

        CustomGameOver.Trigger<CovenGameOver>(winners.Select(player => player.Data).ToArray());
        return true;
    }
}
