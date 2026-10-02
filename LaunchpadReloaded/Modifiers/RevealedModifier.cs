using MiraAPI.Modifiers;
using MiraAPI.Translation;

namespace LaunchpadReloaded.Modifiers;

public class RevealedModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.revealed");

    public override bool HideOnUi => true;
}