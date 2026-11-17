using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Crewmate;

/// <summary>
/// Crewmate that the Impostors see as one of their own (red name, listed in their intro team).
/// While an Infiltrator is in the game, Impostors can kill each other (see ImpostorValidTargetPatch).
/// Can vent, ignores the lights sabotage (InfiltratorPatches) and isn't affected by comms.
/// </summary>
public class InfiltratorRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.InfiltratorColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.CrewmateSupport;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    // Lets the Infiltrator keep venting while comms is sabotaged.
    public override bool IsAffectedByComms => false;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // TODO: replace with dedicated Infiltrator art once it exists in the asset bundle.
        Icon = LaunchpadAssets.HideButton,
        CanUseVent = true,
    };
}
