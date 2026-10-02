using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Crewmate;

public class DetectiveOptions : AbstractOptionGroup<LpDetectiveRole>
{
    public override string GroupName => "launchpad.options.detective";

    [ModdedToggleOption("launchpad.options.detective.hideSuspects")]
    public bool HideSuspects { get; set; } = false;

    [ModdedNumberOption("launchpad.options.detective.suspectCount", 2, 8, 1, MiraNumberSuffixes.None)]
    public float SuspectCount { get; set; } = 4;

    [ModdedNumberOption("launchpad.options.detective.footstepsDuration", 1, 10, 1, MiraNumberSuffixes.Seconds)]
    public float FootstepsDuration { get; set; } = 3;

    [ModdedNumberOption("launchpad.options.detective.instinctDuration", 3, 76, 3, MiraNumberSuffixes.Seconds)]
    public float InstinctDuration { get; set; } = 18;

    [ModdedNumberOption("launchpad.options.detective.instinctUses", 0, 10, zeroInfinity: true)]
    public float InstinctUses { get; set; } = 3;

    [ModdedNumberOption("launchpad.options.detective.instinctCooldown", 0, 45, 1, MiraNumberSuffixes.Seconds)]
    public float InstinctCooldown { get; set; } = 15;

}