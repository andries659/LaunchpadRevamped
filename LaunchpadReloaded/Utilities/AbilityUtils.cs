using System;
using System.Linq;
using AmongUs.GameOptions;
using UnityEngine;

namespace LaunchpadRevamped.Utilities;

/// <summary>
/// Small helpers shared by the roles ported from TOHO.
/// </summary>
public static class AbilityUtils
{
    // Vanilla kill distances for the Short / Medium / Long setting.
    private static readonly float[] KillDistances = [1f, 1.8f, 2.5f];

    /// <summary>The kill range from the lobby settings.</summary>
    public static float KillRange()
    {
        var index = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance), 0, 2);
        return KillDistances[index];
    }

    public static PlayerControl? GetPlayer(byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(player => player && player.PlayerId == id);
    }

    /// <summary>
    /// The closest living player within range that is not an Impostor, optionally narrowed by <paramref name="filter"/>.
    /// </summary>
    public static PlayerControl? ClosestNonImpostor(float range, Func<PlayerControl, bool>? filter = null)
    {
        var local = PlayerControl.LocalPlayer;
        if (!local)
        {
            return null;
        }

        return PlayerControl.AllPlayerControls.ToArray()
            .Where(player => player && !player.AmOwner && player.Data && !player.Data.IsDead && !player.Data.Disconnected
                             && !player.inVent && !player.Data.Role.IsImpostor && (filter == null || filter(player)))
            .Where(player => Vector2.Distance(local.GetTruePosition(), player.GetTruePosition()) <= range)
            .OrderBy(player => Vector2.Distance(local.GetTruePosition(), player.GetTruePosition()))
            .FirstOrDefault();
    }
}
