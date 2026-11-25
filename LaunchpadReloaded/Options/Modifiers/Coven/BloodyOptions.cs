using System;
using LaunchpadRevamped.Modifiers.Game.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers;

public class BloodyOptions : AbstractOptionGroup<BloodyModifier>
{
    public override string GroupName => "launchpad.options.bloody";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<CovenModifierOptions>.Instance.BloodyChance > 0;

    [ModdedNumberOption("launchpad.options.bloody.trailDuration", 3, 30, 1, MiraNumberSuffixes.Seconds)]
    public float TrailDuration { get; set; } = 10;
}
