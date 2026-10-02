using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

public class SurgeonRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.SurgeonColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.DissectButton,
        UseVanillaKillButton = false,
        OptionsScreenshot = LaunchpadAssets.SurgeonBanner,
    };
}