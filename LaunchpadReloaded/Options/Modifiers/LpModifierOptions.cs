using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers;

public class LpModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.modifierOptions";
    public override uint GroupPriority => 0;
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;

    [ModdedNumberOption("launchpad.options.modifierOptions.playerModifierLimit", 0f, 10, 1, suffixType: MiraNumberSuffixes.None, zeroInfinity: true)]
    public float ModifierLimit { get; set; } = 1f;
}