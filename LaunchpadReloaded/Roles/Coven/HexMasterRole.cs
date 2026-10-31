using System;
using System.Collections.Generic;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.GameOver;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Coven;

public class HexMasterRole(IntPtr ptr) : CrewmateRole(ptr), ICovenRole
{
    /// <summary>
    /// Players this Hex Master has hexed since the last meeting. Only tracked on the Hex Master's own client.
    /// </summary>
    public static readonly HashSet<byte> Hexed = [];

    public Color RoleColor => LaunchpadPalette.HexMasterColor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = false,
        Icon = LaunchpadAssets.CovenIcon,
        OptionsScreenshot = LaunchpadAssets.HackerBanner,
    };

    // The Coven only wins together, never with the crew.
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<CovenGameOver>();
    }
}
