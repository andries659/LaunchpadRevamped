using LaunchpadReloaded.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;

namespace LaunchpadReloaded.Options.Roles.Impostor;

public class SurgeonOptions : AbstractOptionGroup<SurgeonRole>
{
    public override string GroupName => "launchpad.options.surgeon";

    [ModdedNumberOption("launchpad.options.surgeon.injectCooldown", 0, 60, 5, MiraNumberSuffixes.Seconds)]
    public float InjectCooldown { get; set; } = 10f;

    [ModdedNumberOption("launchpad.options.surgeon.injectUses", 0, 10, zeroInfinity: true)]
    public float InjectUses { get; set; } = 0;

    [ModdedNumberOption("launchpad.options.surgeon.dissectCooldown", 0, 120, 5, MiraNumberSuffixes.Seconds)]
    public float DissectCooldown { get; set; } = 35f;

    [ModdedNumberOption("launchpad.options.surgeon.dissectUses", 0, 10, zeroInfinity: true)]
    public float DissectUses { get; set; } = 0;

    [ModdedNumberOption("launchpad.options.surgeon.poisonDeathDelay", 5, 60, 5, MiraNumberSuffixes.Seconds)]
    public float PoisonDelay { get; set; } = 10;

    //    [ModdedToggleOption("launchpad.options.surgeon.canUseStandardKill")] NEED FIX
    //    public bool StandardKill { get; set; } = false; NEED FIX
}