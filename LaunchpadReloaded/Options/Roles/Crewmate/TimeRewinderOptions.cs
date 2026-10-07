using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Crewmate;

public class TimeRewinderOptions : AbstractOptionGroup<TimeRewinderRole>
{
    public override string GroupName => "launchpad.options.timeRewinder";

    [ModdedNumberOption("launchpad.options.timeRewinder.rewindCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float RewindCooldown { get; set; } = 45;

    [ModdedNumberOption("launchpad.options.timeRewinder.rewindDuration", 2, 30, 1, MiraNumberSuffixes.Seconds)]
    public float RewindDuration { get; set; } = 10;

    [ModdedNumberOption("launchpad.options.timeRewinder.rewindUses", 0, 8, zeroInfinity: true)]
    public float RewindUses { get; set; } = 3;

    [ModdedToggleOption("launchpad.options.timeRewinder.rewindEveryone")]
    public bool RewindEveryone { get; set; } = true;
}
