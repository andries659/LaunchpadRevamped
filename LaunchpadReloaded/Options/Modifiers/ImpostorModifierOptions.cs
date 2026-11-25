using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Options.Modifiers;

public class ImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.impostorModifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 3;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;

    [ModdedNumberOption("launchpad.options.impostorModifiers.surgeChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float SurgeChance { get; set; } = 0f;
}
