using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.PluginLoading;

namespace LaunchpadRevamped.Modifiers;

[MiraIgnore]
public class VendettaMarkModifier(byte vendettaPlayer) : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.vendettaMark");
    public override bool HideOnUi => true;
    public PlayerControl Vendetta { get; private set; } = GameData.Instance.GetPlayerById(vendettaPlayer).Object;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
