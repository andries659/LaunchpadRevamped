using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class HitmanOptions : AbstractOptionGroup<HitmanRole>
{
    public override string GroupName => "launchpad.options.hitman";

    [ModdedNumberOption("launchpad.options.hitman.deadlockCooldown", 20, 120, 5, MiraNumberSuffixes.Seconds)]
    public float DeadlockCooldown { get; set; } = 40;

    [ModdedNumberOption("launchpad.options.hitman.deadlockUses", 0, 12, 2, zeroInfinity: true)]
    public float DeadlockUses { get; set; } = 3;

    [ModdedNumberOption("launchpad.options.hitman.deadlockMarkLimit", 1, 12, 1, zeroInfinity: false)]
    public float MarkLimit { get; set; } = 2;

    [ModdedNumberOption("launchpad.options.hitman.deadlockDuration", 15, 50, 2.5f)]
    public float DeadlockDuration { get; set; } = 20;
}