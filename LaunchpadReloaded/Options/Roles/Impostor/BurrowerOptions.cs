using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class BurrowerOptions : AbstractOptionGroup<BurrowerRole>
{
    public override string GroupName => "launchpad.options.burrower";

    [ModdedNumberOption("launchpad.options.burrower.ventDigCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float VentDigCooldown { get; set; } = 35;

    [ModdedNumberOption("launchpad.options.burrower.ventDigUses", 0, 12, 2, zeroInfinity: true)]
    public float VentDigUses { get; set; } = 0;

    [ModdedNumberOption("launchpad.options.burrower.minVentDistance", 0, 10, 0.5f)]
    public float VentDist { get; set; } = 1.5f;
}