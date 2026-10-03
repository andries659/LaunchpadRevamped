using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Options.Modifiers;

public class CrewmateModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.crewmateModifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override Color GroupColor => Palette.CrewmateRoleHeaderBlue;

    [ModdedNumberOption("launchpad.options.crewmateModifiers.mayorChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float MayorChance { get; set; } = 0f;

    [ModdedNumberOption("launchpad.options.crewmateModifiers.torchChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float TorchChance { get; set; } = 0f;

    [ModdedNumberOption("launchpad.options.crewmateModifiers.vendettaChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float VendettaChance { get; set; } = 0f;

    [ModdedNumberOption("launchpad.options.crewmateModifiers.chameleonChance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float ChameleonChance { get; set; } = 0f;
}