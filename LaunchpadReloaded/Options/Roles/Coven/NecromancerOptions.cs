using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class NecromancerOptions : AbstractOptionGroup<NecromancerRole>
{
    public override string GroupName => "launchpad.options.necromancer";

    [ModdedNumberOption("launchpad.options.necromancer.reanimateCooldown", 15, 90, 5, MiraNumberSuffixes.Seconds)]
    public float ReanimateCooldown { get; set; } = 45;

    [ModdedNumberOption("launchpad.options.necromancer.reanimations", 1, 5)]
    public float Reanimations { get; set; } = 1;
}
