using System;
using System.Collections.Generic;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.GameOver;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Coven;

/// <summary>
/// Ported from TOHO (Jinx). A jinxed player who gets attacked takes the attacker down with them.
/// </summary>
public class JinxRole(IntPtr ptr) : CrewmateRole(ptr), ICovenRole
{
    /// <summary>Jinxed player id -> the Jinx who cast it. Synced to every client by RpcJinx.</summary>
    public static readonly Dictionary<byte, byte> Jinxed = [];

    public Color RoleColor => LaunchpadPalette.JinxColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.CovenKilling;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = false,
        FreeplayFolder = LaunchpadRoleGroups.CovenFreeplayFolder,
        Icon = LaunchpadAssets.CovenIcon,
    };

    // The Coven only wins together, never with the crew.
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<CovenGameOver>();
    }
}
