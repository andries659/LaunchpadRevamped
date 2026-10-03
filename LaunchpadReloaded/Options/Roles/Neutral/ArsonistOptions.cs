using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Neutral;

public class ArsonistOptions : AbstractOptionGroup<ArsonistRole>
{
    public override string GroupName => "launchpad.options.arsonist";

    [ModdedNumberOption("launchpad.options.arsonist.douseCooldown", 0, 60, 5, MiraNumberSuffixes.Seconds)]
    public float DouseCooldown { get; set; } = 15;

    [ModdedNumberOption("launchpad.options.arsonist.burnCooldown", 0, 60, 5, MiraNumberSuffixes.Seconds)]
    public float BurnCooldown { get; set; } = 10;

    [ModdedNumberOption("launchpad.options.arsonist.dousedNeededToBurn", 1, 8)]
    public float DousedNeededToBurn { get; set; } = 1;
}
