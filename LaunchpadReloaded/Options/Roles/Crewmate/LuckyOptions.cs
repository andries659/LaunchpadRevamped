using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Crewmate;

public class LuckyOptions : AbstractOptionGroup<LuckyRole>
{
    public override string GroupName => "launchpad.options.lucky";

    [ModdedNumberOption("launchpad.options.lucky.dodgeChance", 5, 100, 5, MiraNumberSuffixes.Percent)]
    public float DodgeChance { get; set; } = 35;

    [ModdedNumberOption("launchpad.options.lucky.maxDodges", 0, 5, zeroInfinity: true)]
    public float MaxDodges { get; set; } = 1;
}
