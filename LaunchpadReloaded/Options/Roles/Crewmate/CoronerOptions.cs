using LaunchpadReloaded.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Crewmate;

public class CoronerOptions : AbstractOptionGroup<CoronerRole>
{
    public override string GroupName => "launchpad.options.coroner";

    [ModdedNumberOption("launchpad.options.coroner.freezeCooldown", 0, 50, 5, MiraNumberSuffixes.Seconds)]
    public float FreezeCooldown { get; set; } = 15;

    [ModdedNumberOption("launchpad.options.coroner.freezeUses", 0, 10, zeroInfinity: true)]
    public float FreezeUses { get; set; } = 0;
}