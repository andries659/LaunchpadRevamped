using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Crewmate;

public class SnitchRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.SnitchColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.CrewmateInvestigative;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.SnitchIcon,
    };
}
