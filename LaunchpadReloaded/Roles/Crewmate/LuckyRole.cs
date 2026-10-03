using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Crewmate;

public class LuckyRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.LuckyColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.LuckyIcon,
        OptionsScreenshot = LaunchpadAssets.SheriffBanner,
    };

    /// <summary>
    /// How many kill attempts this Lucky has dodged. Only tracked by the host, which decides dodges.
    /// </summary>
    public int DodgesUsed;
}
