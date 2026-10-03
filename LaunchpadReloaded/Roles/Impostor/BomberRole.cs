using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

public class BomberRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.BomberColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.BomberIcon,
        OptionsScreenshot = LaunchpadAssets.HackerBanner,
    };
}
