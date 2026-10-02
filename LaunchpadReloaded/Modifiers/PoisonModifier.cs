using MiraAPI.Modifiers;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Modifiers;

public class PoisonModifier : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.poisoned");
    public override bool HideOnUi => true;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
