using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class VampireOptions : AbstractOptionGroup<VampireRole>
{
    public override string GroupName => "launchpad.options.vampire";

    [ModdedNumberOption("launchpad.options.vampire.biteCooldown", 0, 120, 2.5f, MiraNumberSuffixes.Seconds)]
    public float BiteCooldown { get; set; } = 25;

    [ModdedNumberOption("launchpad.options.vampire.biteDelay", 1, 60, 1, MiraNumberSuffixes.Seconds)]
    public float BiteDelay { get; set; } = 10;

    [ModdedToggleOption("launchpad.options.vampire.dieAfterMeeting")]
    public bool DieAfterMeeting { get; set; } = true;
}
