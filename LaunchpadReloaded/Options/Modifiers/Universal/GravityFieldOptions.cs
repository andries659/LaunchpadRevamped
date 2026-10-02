using System;
using LaunchpadRevamped.Modifiers.Game.Universal;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers.Universal;

public class GravityFieldOptions : AbstractOptionGroup<GravityModifier>
{
    public override string GroupName => "launchpad.options.gravityField";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<UniversalModifierOptions>.Instance.GravityChance > 0;

    [ModdedNumberOption("launchpad.options.gravityField.gravityFieldRadius", 0.5f, 10f, 0.5f, suffixType: MiraNumberSuffixes.None)]
    public float FieldRadius { get; set; } = 2f;
}