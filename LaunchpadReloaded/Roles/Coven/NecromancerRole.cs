using System;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.GameOver;
using MiraAPI.GameEnd;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Coven;

public class NecromancerRole(IntPtr ptr) : CrewmateRole(ptr), ICovenRole
{
    public Color RoleColor => LaunchpadPalette.NecromancerColor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        UseVanillaKillButton = false,
        CanUseVent = false,
        TasksCountForProgress = false,
        Icon = LaunchpadAssets.CovenIcon,
        OptionsScreenshot = LaunchpadAssets.MedicBanner,
    };

    // The Coven only wins together, never with the crew.
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return gameOverReason == CustomGameOver.GameOverReason<CovenGameOver>();
    }
}
