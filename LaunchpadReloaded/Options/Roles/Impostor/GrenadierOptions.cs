using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Options.Roles.Impostor;

public class GrenadierOptions : AbstractOptionGroup<GrenadierRole>
{
    public override string GroupName => "launchpad.options.grenadier";

    [ModdedNumberOption("launchpad.options.grenadier.blindCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float BlindCooldown { get; set; } = 30;

    [ModdedNumberOption("launchpad.options.grenadier.blindDuration", 1, 30, 1, MiraNumberSuffixes.Seconds)]
    public float BlindDuration { get; set; } = 8;

    [ModdedNumberOption("launchpad.options.grenadier.blindRadius", 0.5f, 10f, 0.5f, suffixType: MiraNumberSuffixes.None)]
    public float BlindRadius { get; set; } = 3.5f;

    [ModdedNumberOption("launchpad.options.grenadier.blindUses", 0, 10, zeroInfinity: true)]
    public float BlindUses { get; set; } = 3;

    [ModdedToggleOption("launchpad.options.grenadier.blindImpostors")]
    public bool BlindImpostors { get; set; } = false;
}
