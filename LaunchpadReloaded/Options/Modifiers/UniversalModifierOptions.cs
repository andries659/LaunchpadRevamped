using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Modifiers;

public class UniversalModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.universalModifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 1;

    [ModdedNumberOption("launchpad.options.universalModifiers.giantChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float GiantChance { get; set; } = 0f;

    [ModdedNumberOption("launchpad.options.universalModifiers.smolChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float SmolChance { get; set; } = 0f;

    [ModdedNumberOption("launchpad.options.universalModifiers.gravityFieldChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float GravityChance { get; set; } = 0f;
}