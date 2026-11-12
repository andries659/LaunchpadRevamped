using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Options.Modifiers;

public class NeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.neutralModifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 4;
    public override Color GroupColor => Color.gray;

    [ModdedNumberOption("launchpad.options.neutralModifiers.testNeutralChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float TestNeutralChance { get; set; } = 0f;
}
