using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class MedusaOptions : AbstractOptionGroup<MedusaRole>
{
    public override string GroupName => "launchpad.options.medusa";

    [ModdedNumberOption("launchpad.options.medusa.gazeCooldown", 15, 90, 2.5f, MiraNumberSuffixes.Seconds)]
    public float GazeCooldown { get; set; } = 40;

    [ModdedNumberOption("launchpad.options.medusa.freezeDuration", 1, 10, 0.5f, MiraNumberSuffixes.Seconds)]
    public float FreezeDuration { get; set; } = 3;

    [ModdedNumberOption("launchpad.options.medusa.gazeRange", 2, 10, 0.5f, suffixType: MiraNumberSuffixes.None)]
    public float GazeRange { get; set; } = 5;
}
