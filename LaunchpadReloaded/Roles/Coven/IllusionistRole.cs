using System;
using System.Collections.Generic;
using System.Linq;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.GameOver;
using LaunchpadRevamped.Options.Roles.Coven;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Coven;

/// <summary>
/// Ported from TOHO (Illusionist). Illusioned players look guilty to investigators (the Snitch). Only the Coven
/// can see who is illusioned.
/// </summary>
public class IllusionistRole(IntPtr ptr) : CrewmateRole(ptr), ICovenRole
{
    /// <summary>Illusioned player id -> the Illusionist who did it. Synced to every client by RpcIllusion.</summary>
    public static readonly Dictionary<byte, byte> Illusioned = [];

    public static bool IsIllusioned(byte playerId) => Illusioned.ContainsKey(playerId);

    /// <summary>True if this player should look like an Impostor to the Snitch.</summary>
    public static bool FoolsSnitch(PlayerControl? player)
    {
        return player && IsIllusioned(player!.PlayerId) && !CovenUtils.IsCoven(player)
               && OptionGroupSingleton<IllusionistOptions>.Instance.SnitchAffected;
    }

    public Color RoleColor => LaunchpadPalette.IllusionistColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.CovenSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = false,
        FreeplayFolder = LaunchpadRoleGroups.CovenFreeplayFolder,
        Icon = LaunchpadAssets.CovenIcon,
    };

    public override void OnDeath(DeathReason reason)
    {
        if (!OptionGroupSingleton<IllusionistOptions>.Instance.ClearWhenDead)
        {
            return;
        }

        foreach (var target in Illusioned.Where(pair => pair.Value == Player.PlayerId).Select(pair => pair.Key).ToArray())
        {
            Illusioned.Remove(target);
        }
    }

    // The Coven only wins together, never with the crew.
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<CovenGameOver>();
    }
}
