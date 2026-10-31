using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class HexMasterOptions : AbstractOptionGroup<HexMasterRole>
{
    public override string GroupName => "launchpad.options.hexMaster";

    [ModdedNumberOption("launchpad.options.hexMaster.hexCooldown", 5, 45, 2.5f, MiraNumberSuffixes.Seconds)]
    public float HexCooldown { get; set; } = 15;

    [ModdedNumberOption("launchpad.options.hexMaster.maxHexes", 1, 5)]
    public float MaxHexes { get; set; } = 2;
}
