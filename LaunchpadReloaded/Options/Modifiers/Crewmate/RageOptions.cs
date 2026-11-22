using System;
using LaunchpadRevamped.Modifiers.Game.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Modifiers.Crewmate;

public class RageOptions : AbstractOptionGroup<RageModifier>
{
    public override string GroupName => "launchpad.options.rage";

    public override Func<bool> GroupVisible =>
        () => OptionGroupSingleton<CrewmateModifierOptions>.Instance.RageChance > 0;

    [ModdedNumberOption("launchpad.options.rage.extraTasks", 1, 10, 1, MiraNumberSuffixes.None)]
    public float ExtraTasks { get; set; } = 4;

    [ModdedNumberOption("launchpad.options.rage.killCooldown", 10, 60, 5, MiraNumberSuffixes.Seconds)]
    public float KillCooldown { get; set; } = 15;
}
