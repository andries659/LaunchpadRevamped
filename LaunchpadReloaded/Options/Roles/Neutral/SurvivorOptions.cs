using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Neutral;

public class SurvivorOptions : AbstractOptionGroup<SurvivorRole>
{
    public override string GroupName => "launchpad.options.survivor";

    [ModdedNumberOption("launchpad.options.survivor.vestCooldown", 5, 90, 5, MiraNumberSuffixes.Seconds)]
    public float VestCooldown { get; set; } = 30;

    [ModdedNumberOption("launchpad.options.survivor.vestDuration", 1, 20, 1, MiraNumberSuffixes.Seconds)]
    public float VestDuration { get; set; } = 8;

    [ModdedNumberOption("launchpad.options.survivor.maxVests", 0, 10, zeroInfinity: true)]
    public float MaxVests { get; set; } = 3;
}
