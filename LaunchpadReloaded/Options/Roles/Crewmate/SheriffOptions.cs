using LaunchpadReloaded.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Crewmate;

public class SheriffOptions : AbstractOptionGroup<SheriffRole>
{
    public override string GroupName => "launchpad.options.sheriff";

    [ModdedNumberOption("launchpad.options.sheriff.shotCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float ShotCooldown { get; set; } = 45;

    [ModdedNumberOption("launchpad.options.sheriff.shotsPerGame", 1, 10)]
    public float ShotsPerGame { get; set; } = 3;

    [ModdedToggleOption("launchpad.options.sheriff.shouldCrewmateDie")]
    public bool ShouldCrewmateDie { get; set; } = false;
}