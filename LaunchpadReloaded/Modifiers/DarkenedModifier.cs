using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Coven;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers;

/// <summary>
/// Applied to a player who finished a task marked by the Dark Fairy. Darkened players and the Dark Fairy know each other.
/// </summary>
public sealed class DarkenedModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.darkened");

    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.darkened.TabDescription");

    private static bool LocalKnowsDarkened()
    {
        var local = PlayerControl.LocalPlayer;
        return local && local.Data && (local.Data.Role is DarkFairyRole || local.HasModifier<DarkenedModifier>());
    }

    public override void FixedUpdate()
    {
        if (!Player || !Player.cosmetics || Player.AmOwner || !LocalKnowsDarkened())
        {
            return;
        }

        Player.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.DarkFairyColor));
    }

    public override void OnDeactivate()
    {
        if (Player && Player.cosmetics && !Player.AmOwner)
        {
            Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.DarkFairyColor));
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
