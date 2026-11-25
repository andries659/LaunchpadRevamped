using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class IllusionistOptions : AbstractOptionGroup<IllusionistRole>
{
    public override string GroupName => "launchpad.options.illusionist";

    [ModdedNumberOption("launchpad.options.illusionist.illusionCooldown", 0, 120, 2.5f, MiraNumberSuffixes.Seconds)]
    public float IllusionCooldown { get; set; } = 30;

    [ModdedNumberOption("launchpad.options.illusionist.maxIllusions", 1, 100)]
    public float MaxIllusions { get; set; } = 5;

    [ModdedToggleOption("launchpad.options.illusionist.snitchAffected")]
    public bool SnitchAffected { get; set; } = false;

    [ModdedToggleOption("launchpad.options.illusionist.resetPerRound")]
    public bool ResetPerRound { get; set; } = false;

    [ModdedToggleOption("launchpad.options.illusionist.clearWhenDead")]
    public bool ClearWhenDead { get; set; } = false;
}
