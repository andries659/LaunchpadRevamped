using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Coven;

public class CovenLeaderOptions : AbstractOptionGroup<CovenLeaderRole>
{
    public override string GroupName => "launchpad.options.covenLeader";

    [ModdedNumberOption("launchpad.options.covenLeader.smiteCooldown", 15, 60, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SmiteCooldown { get; set; } = 30;
}
