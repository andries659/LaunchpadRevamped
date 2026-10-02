using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class HackerOptions : AbstractOptionGroup<HackerRole>
{
    public override string GroupName => "launchpad.options.hacker";

    [ModdedNumberOption("launchpad.options.hacker.hackCooldown", 10, 300, 10, MiraNumberSuffixes.Seconds)]
    public float HackCooldown { get; set; } = 60;

    [ModdedNumberOption("launchpad.options.hacker.hackDuration", 10, 500, 10, MiraNumberSuffixes.Seconds)]
    public float HackDuration { get; set; } = 90;

    [ModdedNumberOption("launchpad.options.hacker.hacksPerGame", 1, 8)]
    public float HackUses { get; set; } = 2;

    [ModdedNumberOption("launchpad.options.hacker.mapCooldown", 0, 40, 3, MiraNumberSuffixes.Seconds)]
    public float MapCooldown { get; set; } = 10;

    [ModdedNumberOption("launchpad.options.hacker.mapDuration", 1, 30, 3, MiraNumberSuffixes.Seconds)]
    public float MapDuration { get; set; } = 3;
}