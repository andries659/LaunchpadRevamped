using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Crewmate;

public class TimeRewinderRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.TimeRewinderColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.TimeRewinderIcon,
        OptionsScreenshot = LaunchpadAssets.MedicBanner,
    };
}
