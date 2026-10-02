using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

public class HackerRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.HackerColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.HackButton,
        OptionsScreenshot = LaunchpadAssets.HackerBanner,
    };
}
