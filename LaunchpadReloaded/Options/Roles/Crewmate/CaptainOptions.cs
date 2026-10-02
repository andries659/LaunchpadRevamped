using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Crewmate;

public class CaptainOptions : AbstractOptionGroup<CaptainRole>
{
    public override string GroupName => "launchpad.options.captain";

    [ModdedNumberOption("launchpad.options.captain.meetingCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float CaptainMeetingCooldown { get; set; } = 45;

    [ModdedNumberOption("launchpad.options.captain.meetingUses", 1, 5)]
    public float CaptainMeetingCount { get; set; } = 3;

    [ModdedNumberOption("launchpad.options.captain.zoomCooldown", 5, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ZoomCooldown { get; set; } = 30;

    [ModdedNumberOption("launchpad.options.captain.zoomDuration", 5, 25, 1, MiraNumberSuffixes.Seconds)]
    public float ZoomDuration { get; set; } = 10;

    [ModdedNumberOption("launchpad.options.captain.zoomDistance", 4, 15)]
    public float ZoomDistance { get; set; } = 6;
}