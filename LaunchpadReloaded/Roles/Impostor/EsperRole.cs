using System;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

/// <summary>
/// Impostor that can project a ghost out of its body (see ProjectButton / EsperProjection).
/// While projected the Esper is invisible, walks through walls and can kill/sabotage, but its body is left behind as a
/// reportable corpse. A meeting starting while projected kills the Esper.
/// </summary>
public class EsperRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    public Color RoleColor => LaunchpadPalette.EsperColor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        // TODO: replace with dedicated Esper art once it exists in the asset bundle.
        Icon = LaunchpadAssets.HideButton,
        OptionsScreenshot = LaunchpadAssets.HackerBanner,
    };

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        // Role swapped away (or game reset) while projected: put everything back.
        if (EsperProjection.IsProjected(targetPlayer))
        {
            EsperProjection.End(targetPlayer, snapToBody: false);
        }

        base.Deinitialize(targetPlayer);
    }
}
