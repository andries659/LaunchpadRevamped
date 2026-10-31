using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class PoisonerOptions : AbstractOptionGroup<PoisonerRole>
{
    public override string GroupName => "launchpad.options.poisoner";

    [ModdedNumberOption("launchpad.options.poisoner.poisonCooldown", 10, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PoisonCooldown { get; set; } = 25;

    [ModdedNumberOption("launchpad.options.poisoner.poisonDelay", 5, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float PoisonDelay { get; set; } = 20;
}
