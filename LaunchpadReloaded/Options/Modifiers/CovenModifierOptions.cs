using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Options.Modifiers;

public class CovenModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.covenModifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 5;
    public override Color GroupColor => LaunchpadRevamped.Features.LaunchpadPalette.CovenColor;

    [ModdedNumberOption("launchpad.options.covenModifiers.testCovenChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float TestCovenChance { get; set; } = 0f;
}
