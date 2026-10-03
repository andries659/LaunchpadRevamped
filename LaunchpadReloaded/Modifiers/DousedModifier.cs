using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers;

/// <summary>
/// Applied by the Arsonist's Douse button. Only the Arsonist can see who is doused.
/// </summary>
public sealed class DousedModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.doused");
    public override bool HideOnUi => true;

    private static bool LocalPlayerIsArsonist()
    {
        return PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data && PlayerControl.LocalPlayer.Data.Role is ArsonistRole;
    }

    public override void FixedUpdate()
    {
        if (!Player || !Player.cosmetics || !LocalPlayerIsArsonist())
        {
            return;
        }

        Player.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.ArsonistColor));
    }

    public override void OnDeactivate()
    {
        if (!Player || !Player.cosmetics || !LocalPlayerIsArsonist())
        {
            return;
        }

        Player.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(LaunchpadPalette.ArsonistColor));
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
