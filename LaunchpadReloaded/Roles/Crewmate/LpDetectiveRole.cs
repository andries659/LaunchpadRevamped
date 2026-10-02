using LaunchpadRevamped.Buttons.Crewmate;
using LaunchpadRevamped.Features;
using MiraAPI.Hud;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Crewmate;

public class LpDetectiveRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.DetectiveColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.InvestigateButton,
        OptionsScreenshot = LaunchpadAssets.DetectiveBanner,
    };

    public override void OnDeath(DeathReason reason)
    {
        Deinitialize(Player);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        if (!targetPlayer.AmOwner)
        {
            return;
        }

        if (CustomButtonSingleton<InstinctButton>.Instance.EffectActive)
        {
            CustomButtonSingleton<InstinctButton>.Instance.OnEffectEnd();
        }
    }
}
