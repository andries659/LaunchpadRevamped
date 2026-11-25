using System;
using System.Collections.Generic;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.GameOver;
using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Roles.Neutral;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Coven;

/// <summary>
/// Ported from TOHO (Dark Fairy). Tasks the Dark Fairy finishes get marked; any other player who finishes a marked
/// task becomes Darkened and fights for the Coven. Marks last until the next meeting ends.
/// </summary>
public class DarkFairyRole(IntPtr ptr) : CrewmateRole(ptr), ICovenRole
{
    /// <summary>Marked task indexes (PlayerTask.Index). Synced to every client by RpcDarkFairyMark.</summary>
    public static readonly HashSet<int> Marked = [];

    /// <summary>Tasks marked this round by the local Dark Fairy.</summary>
    public static int MarkedThisRound;

    public Color RoleColor => LaunchpadPalette.DarkFairyColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.CovenPower;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = false,
        FreeplayFolder = LaunchpadRoleGroups.CovenFreeplayFolder,
        Icon = LaunchpadAssets.CovenIcon,
    };

    public static bool CanBeDarkened(PlayerControl player)
    {
        if (!player || !player.Data || player.Data.IsDead || player.HasModifier<DarkenedModifier>() || CovenUtils.IsCoven(player))
        {
            return false;
        }

        return player.Data.Role is not INeutralRole || OptionGroupSingleton<DarkFairyOptions>.Instance.CanDarkenNeutral;
    }

    // The Coven only wins together, never with the crew.
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<CovenGameOver>();
    }
}
