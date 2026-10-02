using LaunchpadRevamped.Buttons.Crewmate;
using LaunchpadRevamped.Features;
using MiraAPI.Hud;
using MiraAPI.Roles;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Crewmate;

public class CaptainRole(IntPtr ptr) : CrewmateRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.CaptainColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.ZoomButton,
        OptionsScreenshot = LaunchpadAssets.CaptainBanner,
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

        if (CustomButtonSingleton<ZoomButton>.Instance.EffectActive)
        {
            CustomButtonSingleton<ZoomButton>.Instance.OnEffectEnd();
        }
    }
}