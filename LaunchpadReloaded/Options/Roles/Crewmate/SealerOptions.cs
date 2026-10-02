using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Crewmate;

public class SealerOptions : AbstractOptionGroup<SealerRole>
{
    public override string GroupName => "launchpad.options.sealer";

    [ModdedNumberOption("launchpad.options.sealer.sealVentCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float SealVentCooldown { get; set; } = 35;

    [ModdedNumberOption("launchpad.options.sealer.sealVentUses", 0, 10, zeroInfinity: true)]
    public float SealVentUses { get; set; } = 3;

    [ModdedToggleOption("launchpad.options.sealer.sealRevealsBodies")]
    public bool SealReveal { get; set; } = true;
}