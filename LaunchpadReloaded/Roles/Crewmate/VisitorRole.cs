using LaunchpadRevamped.Features;
using LaunchpadRevamped.Utilities;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Crewmate;

public class VisitorRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.VisitorColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.CrewmateInvestigative;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.InvestigateButton,
    };

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        if (targetPlayer.AmOwner)
        {
            VisitorUtilities.Reset();
        }
    }
}
