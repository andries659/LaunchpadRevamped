using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Universal;

/// <summary>
/// Ported from TOHO (Bait). When a Bait is killed by another player, the killer is forced to report the body.
/// The report itself is done in BaitEvents.
/// </summary>
public sealed class BaitModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.bait");

    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.bait.TabDescription");

    public override LoadableAsset<Sprite>? ModifierIcon => LaunchpadAssets.BaitIcon;

    public override Color? ListColor => LaunchpadPalette.BaitColor;

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.BaitChance;

    public override int GetAmountPerGame() => 1;

    // The modifier has to survive the Bait's death, because the self-report is decided right after the murder.
    public override bool RemoveOnDeath => false;
}
