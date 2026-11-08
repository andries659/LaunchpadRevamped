using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.Translation;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using LaunchpadRevamped.Features;

namespace LaunchpadRevamped.Modifiers.Game.Universal;

public sealed class SmolModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.smol");
    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.smol.TabDescription");
    public override LoadableAsset<Sprite>? ModifierIcon => LaunchpadAssets.SmolIcon;
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.SmolChance;
    public override int GetAmountPerGame() => 1;
    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && !role.Player.HasModifier<GiantModifier>();

    public override void OnActivate()
    {
        Player.MyPhysics.Speed /= 0.75f;
        Player.transform.localScale *= 0.7f;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed *= 0.75f;
        Player.transform.localScale /= 0.7f;
    }
}
